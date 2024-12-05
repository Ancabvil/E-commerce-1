import { HttpClient } from '@angular/common/http';
import { Injectable } from '@angular/core';
import { firstValueFrom, Observable } from 'rxjs';
import { ApiService } from './api.service';
import { Result } from '../models/result';
import { User } from '../models/user';
import { ProductDto } from '../models/product-dto';
import { CreateOrUpdateImageRequest } from '../models/create-or-update-image-request';
import { Image } from '../models/image';

@Injectable({
  providedIn: 'root',
})
export class AdminService {
  constructor(private api: ApiService) {}

  async getProducts(): Promise<Result<ProductDto[]>> {
    const products = await this.api.get<ProductDto[]>('Admin/getAllProducts');
    return products;
  }

  async getUsers(): Promise<Result<User[]>> {
    const users = await this.api.get<User[]>('Admin/getAllUsers');
    return users;
  }

  getAllImages(): Promise<Result<Image[]>> {
    return this.api.get<Image[]>('images');
  }

  addImage(
    createOrUpdateImageRequest: CreateOrUpdateImageRequest
  ): Promise<Result<Image>> {
    const formData = new FormData();
    formData.append('name', createOrUpdateImageRequest.name);
    formData.append('file', createOrUpdateImageRequest.file);

    return this.api.post<Image>('images', formData);
  }

  updateImage(id: number, createOrUpdateImageRequest: CreateOrUpdateImageRequest): Promise<Result> {
    const formData = new FormData();
    formData.append('name', createOrUpdateImageRequest.name);
    formData.append('file', createOrUpdateImageRequest.file);

    return this.api.put(`images/${id}`, formData);
  }

  deleteImage(id: number): Promise<Result> {
    return this.api.delete(`images/${id}`);
  }

  async addProduct(product: ProductDto): Promise<Result<void>> {
    const result = await this.api.post<void>('Admin/addProduct', product);
    return result;
  }

  async removeProduct(productId: number): Promise<Result<void>> {
    const result = await this.api.delete<void>(
      `Admin/deleteProduct/${productId}`
    );
    return result;
  }
}
