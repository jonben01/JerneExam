import {
    type BoardDto,
    type BoardPurchaseResponseDto,
    type GameDto,
    type PurchaseBoardRequest,
} from "../generated-ts-client";
import {boardClient, gameClient} from "./clients.ts";


export const getActiveGame = async (): Promise<GameDto> => {

    return await gameClient.getActiveGame();
};

export const purchaseBoard = async (
    selectedNumbers: number[],
    gameId: string
): Promise<BoardPurchaseResponseDto> => {

    const request: PurchaseBoardRequest = {
        gameId,
        numbers: selectedNumbers,
    };

    return await boardClient.purchaseBoard(request);

};

export const getMyBoards = async (gameId?: string): Promise<BoardDto[]> => {

    return await boardClient.getMyBoards(gameId);

};