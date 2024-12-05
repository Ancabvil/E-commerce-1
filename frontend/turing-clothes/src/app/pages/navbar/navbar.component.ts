import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../services/auth.service';
import { RouterLink } from '@angular/router';
import { CartComponent } from '../cart/cart.component';
import { CartServiceService } from '../../services/cart-service.service';
import { Result } from '../../models/result';
import { LocalCartComponent } from '../local-cart/local-cart.component';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [RouterLink],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.css',
})
export class NavbarComponent implements OnInit {
  jsonDecodedName: string;
  cartQuantity: number;
  constructor(
    private authService: AuthService,
    private cartService: CartServiceService
  ) { }
  async ngOnInit(): Promise<void> {
    if (this.isLoguedIn()) {
      await this.cartService.getCart();
    }
    this.cartService.cartQuantity$.subscribe((quantity) => {
      this.cartQuantity = quantity;
    });
  }

  isLoguedIn(): boolean {
    return this.authService.isLoggedIn;
  }

  isAdmin(): boolean {
    return this.authService.decodedToken.role === 'admin';
  }

  getLogedUsername() {
    return this.authService.decodedToken.unique_name;
  }

  logout() {
    this.authService.logout();
  }
}
