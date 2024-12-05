import { CartDetail } from "./cart-detail";
import { User } from "./user";

export interface Order {
  id: number,
  userId: number,
  paymentMethod: string,
  email?: string,
  transactionStatus: string,
  totalPrice: number,
  orderDetails: CartDetail[],
  user: User;
}
