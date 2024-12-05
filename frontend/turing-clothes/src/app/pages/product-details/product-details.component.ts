import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { ProductDto } from '../../models/product-dto';
import { CatalogService } from '../../services/catalog.service';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { ReviewDto } from '../../models/review-dto';
import Swal from 'sweetalert2';
import { FormsModule } from '@angular/forms';
import { User } from '../../models/user';
import { Result } from '../../models/result';
import { CartServiceService } from '../../services/cart-service.service';
import { ChangeDetectorRef } from '@angular/core';
import { Cart } from '../../models/cart';

@Component({
  selector: 'app-product-details',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css',
})
export class ProductDetailsComponent implements OnInit {
  items: ProductDto[] = [];
  product: ProductDto | null = null;
  reviewText: string = '';
  quantity: number = 1;
  productReviews: { [key: number]: number } = {};

  constructor(
    private activatedRoute: ActivatedRoute,
    private catalogService: CatalogService,
    public authService: AuthService,
    private cartService: CartServiceService,
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    const id = this.activatedRoute.snapshot.paramMap.get(
      'id'
    ) as unknown as number;

    const result = await this.catalogService.getProductDetailsById(id);

    this.product = result.data;
    // this.loadProductsAndReviews();
    console.log(this.product);
  }

  //esto duplicaba la url de la imagen, he hecho la versión de arriba que distingue el http y el https para que en caso del primero no la duplique
  // async ngOnInit(): Promise<void> {
  //   const id = this.activatedRoute.snapshot.paramMap.get('id') as unknown as number;

  //   const result = await this.catalogService.getProductDetailsById(id);

  //   result.data.image = `https://localhost:7183/${result.data.image}`;
  //   this.product = result.data;
  //   this.loadProductsAndReviews();
  //   console.log(this.product);
  // }

  loadProductsAndReviews(): void {
    this.loadProductReviews(this.product.id);
  }

  async loadProductReviews(productId: number) {
    const reviews = await this.catalogService.getProductReviews(productId);

    if (reviews.data && reviews.data.length > 0) {
      const totalRating = reviews.data.reduce(
        (acc, review) => acc + (review.rating || 0),
        0
      );
      const averageRating = totalRating / reviews.data.length;

      this.productReviews[productId] = averageRating;
      console.log(
        `Product ID: ${productId}, Calculated Average Rating: ${averageRating}`
      );
    } else {
      this.productReviews[productId] = 0;
      console.log(
        `Product ID: ${productId} has no reviews, setting rating to 0`
      );
    }
  }

  getStarClasses(rating: number): string[] {
    const classes = [];

    if (rating === 1) {
      // Buena: cinco estrellas completas
      classes.push(
        'bi bi-star-fill',
        'bi bi-star-fill',
        'bi bi-star-fill',
        'bi bi-star-fill',
        'bi bi-star-fill'
      );
    } else if (rating === 0) {
      // Regular: tres estrellas completas y dos vacías
      classes.push(
        'bi bi-star-fill',
        'bi bi-star-fill',
        'bi bi-star-fill',
        'bi bi-star',
        'bi bi-star'
      );
    } else if (rating === -1) {
      // Mala: cinco estrellas vacías
      classes.push(
        'bi bi-star',
        'bi bi-star',
        'bi bi-star',
        'bi bi-star',
        'bi bi-star'
      );
    }

    return classes;
  }

  async confirmReview(): Promise<void> {
    if (!this.product) return;

    const review: ReviewDto = {
      productId: this.product.id,
      texto: this.reviewText,
      id: 0,
      userId: '',
      rating: 0,
      dateTime: '',
      user: new User(),
      product: undefined,
    };
    try {
      const result = await this.catalogService.addReview(review);
      if (result.success) {
        Swal.fire('Reseña enviada');
        this.reviewText = '';
        this.product.reviews.push(result.data);
      } else {
        Swal.fire('No se pudo enviar la reseña');
      }
    } catch {
      Swal.fire('Error');
    }
  }
  async buyNow(productId: number, quantity: number): Promise<void> {
    if (quantity > 0) {
      this.addProductToCart(productId, quantity);
      if (this.authService.isLoggedIn) {
        await this.router.navigate(['/cart']);
        await this.cartService.getCart();
        Swal.fire({
          icon: 'success',
          text: 'Producto añadido',
          showConfirmButton: false,
          animation: true,
          toast: true,
          position: 'top-right',
          timer: 1100
        });

      } else {
        await this.router.navigate(['/local-cart']);
      }
    }
  }

  async addProductToCart(
    productId: number,
    quantity: number
  ): Promise<Result<string>> {
    if (!this.authService.isLoggedIn) {
      const localCart: Cart = JSON.parse(localStorage.getItem('localCart')) || {
        details: [],
      };
      const product = (
        await this.catalogService.getProductDetailsById(productId)
      ).data;

      const existingDetail = localCart.details.find(
        (detail) => detail.productId === productId
      );

      if (existingDetail) {
        existingDetail.amount += quantity;
        
      } else {
        const newDetails = {
          id: null,
          productId: product.id,
          product,
          amount: quantity,
          cartId: null,
        };
        localCart.details.push(newDetails);
      }
      localStorage.setItem('localCart', JSON.stringify(localCart));
      
      return Result.success(200, 'Producto agregado al carrito');
      
    }
    const result = await this.cartService.addProductToCart(productId, quantity);
    console.log(result);
    if (result.success) {
      await this.cartService.getCart();
      Swal.fire({
        icon: 'success',
        text: 'Producto añadido',
        showConfirmButton: false,
        animation: true,
        toast: true,
        position: 'top-right',
        timer: 1100
      });

      return result;
    }
    return result;
  }
}
