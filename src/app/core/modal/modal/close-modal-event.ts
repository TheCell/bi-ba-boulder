import { CloseType } from "../close-type";

export interface CloseModalEvent {
    closeType: CloseType;
    data?: unknown;
}