import { Component, OnInit } from '@angular/core';
import { CheckoutSession } from '../../models/checkout-session';
import { CheckoutSessionStatus } from '../../models/checkout-session-status';
import { ActivatedRoute, RouterLink } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { CheckoutService } from '../../services/checkout.service';
import { Order } from '../../models/order';
import { Subscription } from 'rxjs';


@Component({
  selector: 'app-confirm-checkout',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './confirm-checkout.component.html',
  styleUrl: './confirm-checkout.component.css',
})
export class ConfirmCheckoutComponent implements OnInit {
  routeParamMap$: Subscription;
  order: Order = {
    id: 0,
    userId: 0,
    paymentMethod: '',
    email: '',
    transactionStatus: '',
    totalPrice: 0,
    orderDetails: [],
    user: {
      name: '',
      surname: '',
      email: '',
      password: '',
      address: '',
      role: '',
  },
  };

  constructor(
    private route: ActivatedRoute,
    private checkoutService: CheckoutService,
  ) {}

  async ngOnInit(): Promise<void> {
    this.routeParamMap$ = this.route.paramMap.subscribe(async (paramMap) => {
      const id = paramMap.get('id') as unknown as number;
      const result = await this.checkoutService.getOrderById(id);
      this.order = result.data;
      console.log("Datos del pedido:", result.data);
      

      this.order.orderDetails.map((img) => {
        img.product.image = `https://localhost:7183/${img.product.image}`;
      });
    
      this.sendOrderEmail();
    });

  }
  sendOrderEmail() {
    const userName = this.order.user 
        ? `${this.order.user.name} ${this.order.user.surname}` 
        : "Cliente";
    const paymentMethod = this.order.paymentMethod;
    const deliveryAddress = this.order.user?.address || "Dirección no especificada";
    const totalPrice = this.order.totalPrice / 100;
    const totalPriceInEth = paymentMethod === "Ethereum" ? (totalPrice / 1600).toFixed(4) : null;

    const productRows = this.order.orderDetails
        .map(detail => {
            const imageUrl = detail.product.image.startsWith("http") 
                ? detail.product.image 
                : `https://localhost:7183/${detail.product.image}`;
            const unitPrice = (detail.product.price / 100).toFixed(2);
            const totalProductPrice = ((detail.product.price * detail.amount) / 100).toFixed(2);
            return `
                <tr>
                    <td style="padding: 8px; border: 1px solid #ccc; text-align: center;">
                        <img src="${imageUrl}" alt="${detail.product.name}" style="max-width: 50px; max-height: 50px;" />
                    </td>
                    <td style="padding: 8px; border: 1px solid #ccc; text-align: center;">${detail.product.name}</td>
                    <td style="padding: 8px; border: 1px solid #ccc; text-align: center;">${detail.amount}</td>
                    <td style="padding: 8px; border: 1px solid #ccc; text-align: center;">${unitPrice}€</td>
                    <td style="padding: 8px; border: 1px solid #ccc; text-align: center;">${totalProductPrice}€</td>
                </tr>`;
        })
        .join("");

    const emailHtml = `
        <div style="font-family: Arial, sans-serif; color: #333;">
            <h2>Factura de tu compra en Turing Clothes</h2>
            <p>Hola <strong>${userName}</strong>,</p>
            <p>Gracias por tu compra. Aquí tienes el resumen de tu pedido:</p>

            <table style="width: 100%; border-collapse: collapse; margin-top: 20px;">
                <thead>
                    <tr>
                        <th style="padding: 8px; border: 1px solid #ccc; text-align: center;">Imagen</th>
                        <th style="padding: 8px; border: 1px solid #ccc; text-align: center;">Nombre</th>
                        <th style="padding: 8px; border: 1px solid #ccc; text-align: center;">Cantidad</th>
                        <th style="padding: 8px; border: 1px solid #ccc; text-align: center;">Precio unitario</th>
                        <th style="padding: 8px; border: 1px solid #ccc; text-align: center;">Precio total</th>
                    </tr>
                </thead>
                <tbody>
                    ${productRows}
                </tbody>
            </table>

            <p><strong>Método de pago:</strong> ${paymentMethod}</p>
            <p><strong>Total:</strong> ${totalPrice.toFixed(2)}€</p>
            ${
                totalPriceInEth
                    ? `<p><strong>Total en Ethereum:</strong> ${totalPriceInEth} ETH</p>`
                    : ""
            }
            <p><strong>Dirección de entrega:</strong> ${deliveryAddress}</p>

            <p>Gracias por confiar en Turing Clothes.</p>
        </div>
    `;

    this.checkoutService.sendEmail(this.order.email, emailHtml)
        .then(() => {
            console.log("Correo enviado con éxito.");
            alert("¡Correo enviado correctamente!");
        })
        .catch(error => {
            console.error("Error enviando el correo:", error);
            alert("Hubo un error al enviar el correo.");
        });
}




  }

