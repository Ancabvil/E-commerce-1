import { Component } from '@angular/core';
import { Pedido } from '../../models/pedido';
import { NgClass } from '@angular/common';
import { SidebarComponent } from "../../shared/sidebar/sidebar.component";

@Component({
  selector: 'app-admin',
  standalone: true,
  imports: [NgClass, SidebarComponent],
  templateUrl: './admin.component.html',
  styleUrl: './admin.component.css'
})
export class AdminComponent {
  pedidos: Pedido[] = [
    { id: 1234, cliente: 'Juan Pérez', total: 100.0, estado: 'Completado' },
    { id: 1235, cliente: 'María García', total: 75.5, estado: 'Pendiente' }
  ];
}
