import {
  Component,
  ElementRef,
  OnDestroy,
  OnInit,
  ViewChild,
} from '@angular/core';
import {
  StripeEmbeddedCheckout,
  StripeEmbeddedCheckoutOptions,
} from '@stripe/stripe-js';
import { StripeService } from 'ngx-stripe';
import { ProductDto } from '../../models/product-dto';
import { Subscription, timer } from 'rxjs';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { CheckoutService } from '../../services/checkout.service';
import { ApiService } from '../../services/api.service';
import { BlockchainService } from '../../services/blockchain.service';
import { PurchaseInfoDto } from '../../models/purchase-info-dto';
import { CreateEthTransactionRequest } from '../../models/create-eth-transaction-request';
import { CheckTransactionRequest } from '../../models/check-transaction-request';

@Component({
  selector: 'app-checkout',
  standalone: true,
  imports: [],
  templateUrl: './checkout.component.html',
  styleUrl: './checkout.component.css',
})
export class CheckoutComponent implements OnInit, OnDestroy {
  @ViewChild('checkoutDialog')
  checkoutDialogRef: ElementRef<HTMLDialogElement>;

  networkUrl: string = 'https://otter.bordel.wtf/erigon';
  product: ProductDto = null;
  sessionId: string = '';
  pollingSubscription: Subscription;
  stripeEmbedCheckout: StripeEmbeddedCheckout;
  routeQueryMap$: Subscription;
  temporaryOrderId: number;
  payment: string;
  addressToSend: string = "0x5FEB7276Af5FA6b8acA27B03441D9Fe2C5FA6426";
  purchaseInfo: PurchaseInfoDto = {
    temporaryOrder: {
      id: 0,
      userId: 0,
      details: [],
    },
    priceInWei: '',
    ethereumPrice: '',
    totalPrice: 0,
  };

  constructor(
    private checkoutService: CheckoutService,
    private api: ApiService,
    private route: ActivatedRoute,
    private router: Router,
    private stripe: StripeService,
    private blockchainService: BlockchainService
  ) {}
  ngOnDestroy(): void {
    this.pollingSubscription.unsubscribe();
    if (this.payment === "card") {
      this.stripeEmbedCheckout.destroy();
    }
  }

  async ngOnInit() {
    this.temporaryOrderId = this.route.snapshot.queryParamMap.get(
      'temporaryId'
    ) as unknown as number;
    this.sessionId = this.route.snapshot.queryParamMap.get('temporaryId');

    this.payment = this.route.snapshot.queryParamMap.get('payment');
    this.startPolling();

    if (this.payment === 'card') {
      await this.embeddedCheckout();
    }
    await this.getPurchaseInfoDto();
  }

  

  async embeddedCheckout() {
    const request = await this.checkoutService.getEmbededCheckout(
      this.temporaryOrderId,
      this.payment
    );
    this.sessionId = request.data.sessionId;

    if (request.success) {
      const options: StripeEmbeddedCheckoutOptions = {
        onComplete: () => this.goToCorfirmPurchase(this.sessionId),
        clientSecret: request.data.clientSecret,
      };

      this.stripe.initEmbeddedCheckout(options).subscribe((checkout) => {
        this.stripeEmbedCheckout = checkout;
        checkout.mount('#checkout');
        this.checkoutDialogRef.nativeElement.showModal;
      });
    }
  }

  reload() {
    this.router.navigate(['checkout']);
  }

  async startPolling() {
    const pollingInterval = 1500;
    console.log('Refresco...', this.sessionId);
    this.pollingSubscription = timer(0, pollingInterval).subscribe(() => {
      this.pollingRefresh(this.temporaryOrderId);
    });
  }

  async pollingRefresh(temporaryOrderId: number) {
    await this.api.postPolling(
      `TemporaryOrder/RefreshTemporaryOrders?temporaryOrderId=${temporaryOrderId}`
    );
  }

  async goToCorfirmPurchase(sessionId: string) {
    const result = await this.checkoutService.getStatus(
      sessionId,
      this.temporaryOrderId
    );

    if (result.success) {
      this.checkoutDialogRef.nativeElement.close;
      this.router.navigateByUrl(`/confirm-checkout/${result.data.order}`);
    }
  }

  cancelCheckoutDialog() {
    this.stripeEmbedCheckout.destroy();
    this.checkoutDialogRef.nativeElement.close;
  }

  /**
   *  BLOCKCHAIN
   *
   * */

  async getPurchaseInfoDto() {
    const result = await this.blockchainService.getEthereumPrice(
      this.temporaryOrderId
    );
    if (result.success) {
      result.data.temporaryOrder.details.map((detail) => {
        detail.product.image = `https://localhost:7183/${detail.product.image}`;
      });
      this.purchaseInfo = result.data;
    }
  }

  async createTransaction() {
    // Si no está instalado Metamask se lanza un error y se corta la ejecución
    if (!window.ethereum) {
      throw new Error('Metamask not found');
    }

    // Obtener la cuenta de metamask del usuario
    const accounts = await window.ethereum.request({
      method: 'eth_requestAccounts',
    });
    const account = accounts[0];

    // Pedimos permiso al usuario para usar su cuenta de metamask
    await window.ethereum.request({
      method: 'wallet_requestPermissions',
      params: [
        {
          eth_accounts: { account },
        },
      ],
    });

    // Obtenemos los datos que necesitamos para la transacción:
    // gas, precio del gas y el valor en Ethereum
    const transactionRequest: CreateEthTransactionRequest = {
      temporaryOrderId: this.temporaryOrderId
    };
    const ethereumInfoResult = await this.blockchainService.createEthTransaction(
      transactionRequest
    );
    const ethereumInfo = ethereumInfoResult.data;

    // Creamos la transacción y pedimos al usuario que la firme
    const transactionHash = await window.ethereum.request({
      method: 'eth_sendTransaction',
      params: [
        {
          from: account,
          to: this.addressToSend,
          value: ethereumInfo.value,
          gas: ethereumInfo.gas,
          gasPrice: ethereumInfo.gasPrice,
        },
      ],
    });

    // Pedimos al servidor que verifique la transacción.
    // CUIDADO: si el cliente le manda todos los datos,
    // podría engañar al servidor.
    const checkTransactionRequest: CheckTransactionRequest = {
      hash: transactionHash,
      temporaryOrderId: this.temporaryOrderId,
      wallet: account,
      paymentMethod: this.payment,
    };

    const checkTransactionResult = await this.blockchainService.checkTransaction(
      checkTransactionRequest
    );

    // Notificamos al usuario si la transacción ha sido exitosa o si ha fallado.
    if (checkTransactionResult.success && checkTransactionResult.data) {
      alert('Transacción realizada con éxito');
      this.router.navigateByUrl(`/confirm-checkout/${this.temporaryOrderId}`);
    } else {
      alert('Transacción fallida');
    }
  }
}

declare global {
  interface Window {
    ethereum: any;
  }
}
