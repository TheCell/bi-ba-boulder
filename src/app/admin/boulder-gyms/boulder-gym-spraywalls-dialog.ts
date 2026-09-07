import { Component, computed, inject, output, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { BoulderGymDto, BoulderGymService, SpraywallDto, SpraywallsService } from '@api-net/index';
import { CloseModalEvent } from '../../core/modal/modal/close-modal-event';
import { IModal } from '../../core/modal/modal/modal.interface';
import { ToastService } from '../../core/toast-container/toast.service';
import { forkJoin } from 'rxjs';
import { BoulderGymSpraywallDialogData } from './boulder-gym-spraywall-data';

@Component({
  selector: 'app-boulder-gym-spraywalls-dialog',
  imports: [FormsModule],
  templateUrl: './boulder-gym-spraywalls-dialog.html',
  styleUrl: './boulder-gym-spraywalls-dialog.scss'
})
export class BoulderGymSpraywallsDialog implements IModal {
  private boulderGymService = inject(BoulderGymService);
  private spraywallsService = inject(SpraywallsService);
  private toastService = inject(ToastService);

  public closeModal = output<CloseModalEvent>();

  public canCloseWithoutPermission = true;
  public boulderGym = signal<BoulderGymDto | undefined>(undefined);
  public allSpraywalls = signal<SpraywallDto[]>([]);
  public currentlySelectedSpraywalls = signal<SpraywallDto[]>([]);
  public selectedSpraywallIdToAdd = signal<string>('');
  public isLoading = signal<boolean>(false);

  public assignedSpraywalls = computed(() => this.currentlySelectedSpraywalls());
  public availableSpraywalls = computed(() => {
    const assignedIds = new Set(this.currentlySelectedSpraywalls().map((s) => s.id));
    return this.allSpraywalls().filter((s) => !assignedIds.has(s.id));
  });

  public initialize(data: BoulderGymSpraywallDialogData): void {
    this.boulderGym.set(data.boulderGym);
    this.currentlySelectedSpraywalls.set(data.boulderGym.spraywalls ?? []);
    this.loadAvailableSpraywalls();
  }

  public onAddSpraywall(): void {
    const spraywallId = this.selectedSpraywallIdToAdd();
    const spraywall = this.allSpraywalls().find((s) => s.id === spraywallId);
    if (!spraywall) {
      return;
    }

    this.currentlySelectedSpraywalls.update((spraywalls) => [...spraywalls, spraywall]);
    this.selectedSpraywallIdToAdd.set('');
  }

  public onRemoveSpraywall(spraywall: SpraywallDto): void {
    this.currentlySelectedSpraywalls.update((spraywalls) => spraywalls.filter((s) => s.id !== spraywall.id));
  }

  public onSaveAndClose(): void {
    this.isLoading.set(true);
    const unmodifiedSpraywalls = this.boulderGym()?.spraywalls ?? [];
    const targetSpraywalls = this.currentlySelectedSpraywalls();
    const spraywallsToAdd: SpraywallDto[] = targetSpraywalls.filter(
      (sw) => !unmodifiedSpraywalls.some((usw) => usw.id === sw.id)
    );
    const spraywallsToRemove: SpraywallDto[] = unmodifiedSpraywalls.filter(
      (usw) => !targetSpraywalls.some((sw) => sw.id === usw.id)
    );

    const addRequests = spraywallsToAdd.map((sw) =>
      this.boulderGymService.addSpraywallToBoulderGym(this.boulderGym()?.id ?? '', {
        boulderGymId: this.boulderGym()?.id ?? '',
        spraywallId: sw.id
      })
    );
    const removeRequests = spraywallsToRemove.map((sw) =>
      this.boulderGymService.removeSpraywallFromBoulderGym(this.boulderGym()?.id ?? '', sw.id)
    );

    if (addRequests.length + removeRequests.length === 0) {
      this.isLoading.set(false);
      this.closeModal.emit({ closeType: 0, data: this.boulderGym() });
      return;
    }

    forkJoin([...addRequests, ...removeRequests]).subscribe({
      next: () => {
        this.isLoading.set(false);
        this.toastService.showSuccess(
          'Spraywalls updated',
          'Spraywalls were successfully updated for the boulder gym.'
        );
        this.closeModal.emit({ closeType: 0, data: this.boulderGym() });
      },
      error: () => {
        this.isLoading.set(false);
      }
    });
  }

  private loadAvailableSpraywalls(): void {
    this.spraywallsService.getSpraywalls().subscribe({
      next: (spraywalls: SpraywallDto[]) => {
        this.allSpraywalls.set(spraywalls);
      }
    });
  }
}
