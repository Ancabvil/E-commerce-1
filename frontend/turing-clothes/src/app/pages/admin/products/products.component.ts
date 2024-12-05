import { Component, OnInit } from '@angular/core';
import { SidebarComponent } from '../../../shared/sidebar/sidebar.component';
import { ProductDto } from '../../../models/product-dto';
import { AdminService } from '../../../services/admin.service';
import { RouterLink } from '@angular/router';

@Component({
  selector: 'app-products',
  standalone: true,
  imports: [SidebarComponent, RouterLink],
  templateUrl: './products.component.html',
  styleUrl: './products.component.css',
})
export class ProductsComponent implements OnInit {
  products: ProductDto[] = [];

  constructor(private adminService: AdminService) {}

  ngOnInit(): void {
    this.getProducts();
  }
  async getProducts(): Promise<void> {
    const result = await this.adminService.getProducts();
    if (result.success) {
      this.products = result.data;
      this.products = result.data.map((product) => ({
        ...product,
        image: `https://localhost:7183/${product.image}`,
      }));
    }
  }

  async removeProduct(productId: number) {
    const result = await this.adminService.removeProduct(productId);

    if (result.success) {
      await this.getProducts();
    }
  }
}
