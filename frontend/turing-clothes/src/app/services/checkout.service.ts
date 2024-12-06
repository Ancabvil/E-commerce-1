import { Injectable } from '@angular/core';
import { ApiService } from './api.service';
import { CheckoutSession } from '../models/checkout-session';
import { Result } from '../models/result';
import { CheckoutSessionStatus } from '../models/checkout-session-status';
import { ProductDto } from '../models/product-dto';
import { Order } from '../models/order';
import { User } from '../models/user';

@Injectable({
  providedIn: 'root'
})
export class CheckoutService {

  constructor(private api: ApiService) { }

  getAllProducts(temporaryOrderId: number): Promise<Result<ProductDto[]>> {
    return this.api.get<ProductDto[]>(`Checkout/GetAllProductsDto?temporaryOrderId=${temporaryOrderId}`);
  }

  getEmbededCheckout(temporaryOrderId: number, paymentMethod: string): Promise<Result<CheckoutSession>> {
    return this.api.get<CheckoutSession>(`Checkout/embedded?temporaryOrderId=${temporaryOrderId}&paymentMethod=${paymentMethod}`);
  }

  getStatus(sessionId: string, temporaryOrderId: number): Promise<Result<CheckoutSessionStatus>> {
    return this.api.get<CheckoutSessionStatus>(`Checkout/status/${sessionId}?temporaryOrderId=${temporaryOrderId}`);
  }

  getOrderById(orderId: number) {
    return this.api.get<Order>(`Order/GetOrderById?orderId=${orderId}`)
  }

  sendEmail(to: string, htmlContent: string): Promise<any> {
    return this.api.post('Checkout/SendEmail', { to, htmlContent });
}


async getUserByEmail(email: string): Promise<User> {
  const result = await this.api.get<User>(`Auth/user by email?mail=${email}`);
  return result.data;
}

}
