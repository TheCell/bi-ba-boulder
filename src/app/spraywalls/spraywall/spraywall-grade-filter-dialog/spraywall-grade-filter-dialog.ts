import { ChangeDetectionStrategy, Component, inject, OnDestroy, output } from '@angular/core';
import { IModal } from 'src/app/core/modal/modal/modal.interface';
import { SpraywallGradeFilterDialogData } from './spraywall-grade-filter-dialog-data';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormsModule, ReactiveFormsModule } from '@angular/forms';
import { GreaterThanValidator } from 'src/app/core/validators/greater-than-validator';
import { Subscription } from 'rxjs';
import { SpraywallGradeFilterDialogCloseData } from './spraywall-grade-filter-dialog-close-data';
import { CloseModalEvent } from 'src/app/core/modal/modal/close-modal-event';

interface ISpraywallFilterForm {
  gradeMin: number | undefined;
  gradeMax: number | undefined;
}

@Component({
  selector: 'app-spraywall-grade-filter-dialog',
  imports: [CommonModule, FormsModule, ReactiveFormsModule],
  templateUrl: './spraywall-grade-filter-dialog.html',
  styleUrl: './spraywall-grade-filter-dialog.scss',
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class SpraywallGradeFilterDialog implements IModal, OnDestroy {
  private _fb = inject(FormBuilder);
  
  public closeModal = output<CloseModalEvent>();

  public canCloseWithoutPermission = true;

  public filterForm = this._fb.nonNullable.group<ISpraywallFilterForm>({
    gradeMin: (undefined),
    gradeMax: (undefined)
  }, {
    validators: [GreaterThanValidator("gradeMax", "gradeMin")]
  });

  private initializeData?: SpraywallGradeFilterDialogData;
  private subscription: Subscription = new Subscription();

  public constructor() {
    this.subscription.add(this.filterForm.valueChanges.subscribe({
      next: () => {
        if (this.filterForm.dirty) {
          this.canCloseWithoutPermission = true;

          if (this.initializeData) {
            if (this.filterForm.controls.gradeMax?.value !== this.initializeData.maxGrade) {
              this.canCloseWithoutPermission = false;
            }
            if (this.filterForm.controls.gradeMin?.value !== this.initializeData.minGrade) {
              this.canCloseWithoutPermission = false;
            }
          } else {
            this.canCloseWithoutPermission = false;
          }
        }
      }
    }));
  }

  public ngOnDestroy(): void {
    this.subscription.unsubscribe();
  }

  public initialize(data: SpraywallGradeFilterDialogData): void {
    this.initializeData = data;
    this.filterForm.patchValue({
      gradeMax: data.maxGrade,
      gradeMin: data.minGrade
    }, { emitEvent: false });
  }

  public onResetFilter(): void {
    this.filterForm.markAsDirty();
    this.filterForm.patchValue({
      gradeMax: undefined,
      gradeMin: undefined
    });
  }

  // public onClose(): void {
  //   console.log('onClose');
  // }

  public onSubmit(): void {
    // this.isLoading = true;
    // this.filterForm.disable();
    // const postRegisterRequest: PutCreateRequest = {
      //   name: this.filterForm.controls.name.value!,
      //   description: this.filterForm.controls.description?.value,
      //   image: this.imageData!,
      //   fontGrade: this.filterForm.controls.fontGrade?.value,
      //   // password: this.filterForm.controls.password.value,
      // };
      
      // // console.log(postRegisterRequest);
      // this.spraywallsService.putCreate(this.spraywallId, postRegisterRequest).subscribe({
        //   next: () => {
          //     this.isLoading = false;
          //     this.filterForm.reset();
          //     this.modalService.close(0);
          //     this.toastService.showSuccess('Saved Successfully', 'You have successfully saved the spraywall.');
          //   },
          //   error: () => {
            //     this.isLoading = false;
            //     this.filterForm.enable();
            //   }
            // });
    if (this.filterForm.valid) {
      this.canCloseWithoutPermission = true;
      const data: SpraywallGradeFilterDialogCloseData = {
        maxGrade: this.filterForm.controls.gradeMax?.value ?? undefined,
        minGrade: this.filterForm.controls.gradeMin?.value ?? undefined
      };

      this.closeModal.emit({ closeType: 0, data: data });
    }
  }
}
