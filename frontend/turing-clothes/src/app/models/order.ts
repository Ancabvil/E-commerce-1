import { CartDetail } from "./cart-detail";

export interface Order {
  id: number,
  userId: number,
  paymentMethod: string,
  email?: string,
  transactionStatus: string,
  totalPrice: number,
  orderDetails: CartDetail[],
}
