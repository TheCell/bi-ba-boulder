import { ChangeDetectorRef, Component, DestroyRef, inject, OnDestroy, OnInit, signal, ViewChild } from '@angular/core';
import {
  ActivatedRouteSnapshot,
  NavigationEnd,
  Router,
  RouterLink,
  RouterLinkActive,
  RouterOutlet
} from '@angular/router';
import { filter, Subscription } from 'rxjs';
import { OutdoorAreaDto, SectorDto } from '@api-net/index';
import { Modal } from './core/modal/modal/modal';
import { ModalService } from './core/modal/modal.service';
import { LoginDialogComponent } from './core/modal/login-dialog/login-dialog.component';
import { ToastContainer } from './core/toast-container/toast-container';
import { LoginTrackerService } from './auth/login-tracker.service';
import { AuthSessionStateService } from './auth/auth-session-state.service';
import { Icon } from './core/icon/icon';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { OutdoorNavigationItem } from './outdoor-navigation-item.interface';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet, RouterLink, RouterLinkActive, Modal, ToastContainer, Icon],
  templateUrl: './app.component.html',
  styleUrl: './app.component.scss'
})
export class AppComponent implements OnInit, OnDestroy {
  @ViewChild('modal') private modal!: Modal;
  public loginTrackerService = inject(LoginTrackerService);
  public authSessionStateService = inject(AuthSessionStateService);
  public changeDetectorRef = inject(ChangeDetectorRef);
  private modalService = inject(ModalService);
  private router = inject(Router);
  private subscription = new Subscription();
  private destroyRef = inject(DestroyRef);

  public title = 'bibaboulder';
  public outdoorNavigationItems = signal<readonly OutdoorNavigationItem[]>([]);

  public ngOnInit(): void {
    this.subscription.add(
      this.loginTrackerService.authStateChanged$.subscribe(() => {
        this.changeDetectorRef.markForCheck();
      })
    );
    this.subscription.add(
      this.router.events
        .pipe(
          takeUntilDestroyed(this.destroyRef),
          filter((event): event is NavigationEnd => event instanceof NavigationEnd)
        )
        .subscribe(() => {
          this.updateOutdoorNavigation();
        })
    );
    this.updateOutdoorNavigation();
    this.loginTrackerService.checkSession();
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public openLoginModal(): void {
    this.modalService.open(this.modal.id, LoginDialogComponent);
  }

  public logout(): void {
    this.loginTrackerService.logout();
  }

  private updateOutdoorNavigation(): void {
    let routeSnapshot: ActivatedRouteSnapshot = this.router.routerState.snapshot.root;
    while (routeSnapshot.firstChild) {
      routeSnapshot = routeSnapshot.firstChild;
    }

    const outdoorArea = routeSnapshot.data['outdoorArea'] as OutdoorAreaDto | undefined;
    const sector = routeSnapshot.data['sector'] as SectorDto | undefined;
    const outdoorAreaId = outdoorArea?.id;
    const sectorId = sector?.id;
    const navigationItems: OutdoorNavigationItem[] = [];

    if (outdoorArea && outdoorAreaId) {
      navigationItems.push({
        label: outdoorArea.name,
        routerLink: ['/', 'outdoor-area', outdoorAreaId],
        isArea: true,
        isSector: false
      });
    }

    if (sector && sectorId) {
      navigationItems.push({
        label: sector.name,
        routerLink: outdoorAreaId
          ? ['/', 'outdoor-area', outdoorAreaId, 'sector', sectorId]
          : ['/', 'sectors', sectorId],
        isArea: false,
        isSector: true
      });
    }

    this.outdoorNavigationItems.set(navigationItems);
  }
}
