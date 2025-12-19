import {
    type CreateDepositRequest,
    type PendingDepositsListItemDto,
} from "../generated-ts-client";
import { transactionClient } from "./clients.ts";

export const createDepositRequest = async (
    amountDkk: number,
    mobilePayReference: string
): Promise<void> => {
    const request: CreateDepositRequest = {
        amountDkk,
        mobilePayReference,
    };

    await transactionClient.createDepositRequest(request);
};

export const getMyBalance = async (): Promise<number> => {
    return await transactionClient.getMyBalance();
};

export const getPendingDeposits = async (): Promise<
    PendingDepositsListItemDto[]
> => {
    return await transactionClient.getPendingDeposits();
};

export const approveDeposit = async (transactionId: string): Promise<void> => {
    await transactionClient.approveDeposit(transactionId);
};

export const rejectDeposit = async (transactionId: string): Promise<void> => {
    await transactionClient.rejectDeposit(transactionId);
};