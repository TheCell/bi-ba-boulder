export interface IToast {
  title: string;
  message: string;
  // header?: string;
  classname?: 'success' | 'danger' | 'warning';
  delay?: number;
}