import { Component, ElementRef, ViewChild } from '@angular/core';
import { ProductDto } from '../../../../models/product-dto';
import {
  FormBuilder,
  FormGroup,
  FormsModule,
  Validators,
} from '@angular/forms';
import { AdminService } from '../../../../services/admin.service';
import { CreateOrUpdateImageRequest } from '../../../../models/create-or-update-image-request';
import { Image } from '../../../../models/image';

@Component({
  selector: 'app-add-new-product',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './add-new-product.component.html',
  styleUrl: './add-new-product.component.css',
})
export class AddNewProductComponent {
  product: ProductDto = {
    id: 0,
    name: '',
    description: '',
    price: 0,
    stock: 0,
    image: '',
    reviews: [],
  };
  @ViewChild('addEditDialog')
  addOrEditDialog: ElementRef<HTMLDialogElement>;

  @ViewChild('removeDialog')
  deleteDialog: ElementRef<HTMLDialogElement>;

  images: Image[] = [];

  addOrEditForm: FormGroup;
  imageToEdit: Image = null;
  imageToDelete: Image = null;
  constructor(private adminService: AdminService) {}
  private formBuilder: FormBuilder;

  async addProduct(product: ProductDto) {
    product.price = product.price * 100;
    const result = await this.adminService.addProduct(product);
    if (result.success) {
      alert('Producto añadido correctamente');
    }
  }

  /**
   * IMÁGENES
   */

  async upateImageList() {
    const request = await this.adminService.getAllImages();

    if (request.success) {
      this.images = request.data;
    }
  }

  openDialog(dialogRef: ElementRef<HTMLDialogElement>) {
    dialogRef.nativeElement.showModal();
  }

  closeDialog(dialogRef: ElementRef<HTMLDialogElement>) {
    dialogRef.nativeElement.close();
  }

  onFileSelected(event: any) {
    const image = event.target.files[0] as File; // Here we use only the first file (single file)
    this.addOrEditForm.patchValue({ file: image });
  }

  addImage() {
    this.imageToEdit = null;
    this.addOrEditForm = this.formBuilder.group({
      name: ['', Validators.required],
      file: [null, Validators.required],
    });

    this.openDialog(this.addOrEditDialog);
  }

  editImage(image: Image) {
    this.imageToEdit = image;
    this.addOrEditForm = this.formBuilder.group({
      name: [image.name, Validators.required],
      file: [null],
    });

    this.openDialog(this.addOrEditDialog);
  }

  async createOrUpdateImage() {
    const createOrUpdateImageRequest: CreateOrUpdateImageRequest = {
      name: this.addOrEditForm.get('name')?.value,
      file: this.addOrEditForm.get('file')?.value as File,
    };

    // Añadir nueva imagen
    if (this.imageToEdit == null) {
      const request = await this.adminService.addImage(
        createOrUpdateImageRequest
      );

      if (request.success) {
        alert('Imagen añadida con éxito');
        this.closeDialog(this.addOrEditDialog);
        this.upateImageList();
      } else {
        alert(`Ha ocurrido un error: ${request.error}`);
      }
    }
    // Actualizar imagen existente
    else {
      const request = await this.adminService.updateImage(
        this.imageToEdit.id,
        createOrUpdateImageRequest
      );

      if (request.success) {
        alert('Imagen actualizada con éxito');
        this.closeDialog(this.addOrEditDialog);
        this.upateImageList();
      } else {
        alert(`Ha ocurrido un error: ${request.error}`);
      }
    }
  }

  deleteImage(image: Image) {
    this.imageToDelete = image;
    this.openDialog(this.deleteDialog);
  }

  async confirmDeleteImage() {
    const request = await this.adminService.deleteImage(this.imageToDelete.id);

    if (request.success) {
      alert('Se ha borrado la imagen');
      this.imageToDelete = null;
      this.closeDialog(this.deleteDialog);
      this.upateImageList();
    } else {
      alert(`Ha ocurrido un error: ${request.error}`);
    }
  }
}
