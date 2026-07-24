import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { Icon } from '../../core/icon/icon';
import { CameraControlsService } from '../../renderer/camera-controls.service';

@Component({
  selector: 'app-camera-controls',
  imports: [Icon],
  templateUrl: './camera-controls.html',
  styleUrl: './camera-controls.scss',
  changeDetection: ChangeDetectionStrategy.OnPush
})
export class CameraControls {
  public cameraControlsService = inject(CameraControlsService);
}
