import { Routes } from '@angular/router';
import { HomeComponent } from './components/home/home.component';
import { CatalogoComponent } from './components/catalogo/catalogo.component';
import { RegistroComponent } from './components/registro/registro.component';
import { InicioSesionComponent } from './components/iniciosesion/iniciosesion.component';
import { FooterComponent } from './components/footer/footer.component';

export const routes: Routes = [
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent },
  { path: 'catalog', component: CatalogoComponent },
  { path: 'register', component: RegistroComponent },
  { path: 'login', component: InicioSesionComponent },
  { path: 'footer', component: FooterComponent}
];
