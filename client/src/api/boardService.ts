import {
    type BoardPurchaseResponseDto,
    type GameDto,
    type PurchaseBoardRequest, type BoardDto,
} from "../generated-ts-client";
import {boardClient, gameClient} from "./clients.ts";


export const getActiveGame = async (): Promise<GameDto> => {

    const response = await gameClient.getActiveGame();
    return response;
};

export const purchaseBoard = async (
    selectedNumbers: number[],
    gameId: string
): Promise<BoardPurchaseResponseDto> => {

    const request: PurchaseBoardRequest = {
        gameId,
        numbers: selectedNumbers,
    };

    const response = await boardClient.purchaseBoard(request);
    return response;

};

export const getMyBoards = async (gameId?: string): Promise<BoardDto[]> => {

    const response = await boardClient.getMyBoards(gameId);
    return response;

};