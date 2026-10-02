import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BlocDto, BlocsService } from '@api-net/index';
import { of } from 'rxjs';
import { ToastService } from '../../core/toast-container/toast.service';
import { BlocEditorDialog } from './bloc-editor-dialog';

describe('BlocEditorDialog', (): void => {
  let fixture: ComponentFixture<BlocEditorDialog>;
  let component: BlocEditorDialog;
  let blocsService: { createBloc: jasmine.Spy; updateBloc: jasmine.Spy };
  const bloc: BlocDto = { id: 'b1', name: 'New bloc', version: 2, sectorId: null };

  beforeEach(async (): Promise<void> => {
    blocsService = {
      createBloc: jasmine.createSpy('createBloc'),
      updateBloc: jasmine.createSpy('updateBloc')
    };
    blocsService.createBloc.and.returnValue(of(bloc));
    blocsService.updateBloc.and.returnValue(of(bloc));

    await TestBed.configureTestingModule({
      imports: [BlocEditorDialog],
      providers: [
        { provide: BlocsService, useValue: blocsService },
        { provide: ToastService, useValue: jasmine.createSpyObj<ToastService>('ToastService', ['showSuccess']) }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(BlocEditorDialog);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach((): void => {
    fixture.destroy();
  });

  it('creates a bloc without a sector using null in the request', (): void => {
    component.initialize({ sectors: [] });
    component.blocForm.name().value.set(bloc.name);

    expect(component.blocForm().valid()).toBeTrue();
    component.onSubmit();

    expect(blocsService.createBloc).toHaveBeenCalledWith(jasmine.objectContaining({ name: bloc.name, sectorId: null }));
  });

  it('allows removing an existing sector assignment', (): void => {
    component.initialize({ bloc: { ...bloc, sectorId: 's1' } });
    component.blocForm.sectorId().value.set('');

    component.onSubmit();

    expect(blocsService.updateBloc).toHaveBeenCalledWith(
      bloc.id,
      jasmine.objectContaining({ sectorId: null, version: bloc.version })
    );
  });

  it('preserves a selected sector when creating a bloc', (): void => {
    component.initialize({ sectorId: 's1' });
    component.blocForm.name().value.set(bloc.name);

    component.onSubmit();

    expect(blocsService.createBloc).toHaveBeenCalledWith(jasmine.objectContaining({ sectorId: 's1' }));
  });

  it('still requires a name', (): void => {
    component.initialize({});

    component.onSubmit();

    expect(component.blocForm().invalid()).toBeTrue();
    expect(blocsService.createBloc).not.toHaveBeenCalled();
  });
});
