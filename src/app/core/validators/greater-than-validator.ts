import { AbstractControl, ValidatorFn } from "@angular/forms";

export function GreaterThanValidator(controlName: string, comparedToControlName: string): ValidatorFn {
    return (group: AbstractControl) => {
        const control = group.get(controlName);
        const matchingControl = group.get(comparedToControlName);

        if (!control || !matchingControl) {
            return null;
        }

        if (!control.value || !matchingControl.value) {
            return null;
        }

        if (matchingControl.errors && !matchingControl.errors['mustBeGreater']) {
            return null;
        }

        if (control.value <= matchingControl.value) {
            control.setErrors({ mustBeGreater: true });
        } else {
            control.setErrors(null)
        }
        
        return null;
    }
}