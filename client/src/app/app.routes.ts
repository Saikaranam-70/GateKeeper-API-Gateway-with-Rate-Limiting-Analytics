import { Routes } from '@angular/router';
import { LoginComponent } from './components/login/login.component';
import { RegisterComponent } from './components/register/register.component';
import { DashboardComponent } from './components/dashboard/dashboard.component';
import { GatewayFormComponent } from './components/gateway-form/gateway-form.component';
import { GatewayDetailComponent } from './components/gateway-detail/gateway-detail.component';
import { ApiKeysComponent } from './components/api-keys/api-keys.component';
import { SystemStatusComponent } from './components/system-status/system-status.component';
import { authGuard } from './guards/auth.guard';

export const routes: Routes = [
  { path: 'login', component: LoginComponent },
  { path: 'register', component: RegisterComponent },
  { 
    path: '', 
    canActivate: [authGuard],
    children: [
      { path: '', redirectTo: 'dashboard', pathMatch: 'full' },
      { path: 'dashboard', component: DashboardComponent },
      { path: 'gateways/new', component: GatewayFormComponent },
      { path: 'gateways/:id', component: GatewayDetailComponent },
      { path: 'api-keys', component: ApiKeysComponent },
      { path: 'system-status', component: SystemStatusComponent }
    ]
  },
  { path: '**', redirectTo: 'dashboard' }
];
