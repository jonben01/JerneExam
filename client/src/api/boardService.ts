import {
    type BoardPurchaseResponseDto,
    type GameDto,
    type PurchaseBoardRequest, type BoardDto,
} from "../generated-ts-client";
import {boardClient, gameClient} from "./clients.ts";


export const getActiveGame = async (): Promise<GameDto> => {
    try {
        const response = await gameClient.getActiveGame();
        return response;
    } catch (error) {
        throw error;
    }
};

export const purchaseBoard = async (
    selectedNumbers: number[],
    gameId: string
): Promise<BoardPurchaseResponseDto> => {
    try {

        const request: PurchaseBoardRequest = {
            gameId,
            numbers: selectedNumbers,
        };

        const response = await boardClient.purchaseBoard(request);
        return response;
    } catch (error) {
        throw error;
    }
};

export const getMyBoards = async (gameId?: string): Promise<BoardDto[]> => {
    try {

        const response = await boardClient.getMyBoards(gameId);
        return response;
    } catch (error) {
        throw error;
    }
};

export const getMyWinningBoards = async (): Promise<BoardDto[]> => {
    try {

        const response = await boardClient.getMyWinningBoards();
        return response;
    } catch (error) {
        throw error;
    }
};