export interface IToast {
  title: string;
  message: string;
  classname?: 'success' | 'danger' | 'warning';
  delay?: number;
}