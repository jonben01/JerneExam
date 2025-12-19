import {type GameDto, type PublishWinningNumbersRequest,} from "../generated-ts-client";
import {gameClient} from "./clients.ts";

export const publishWinningNumbers = async (
    gameId: string,
    winningNumber1: number,
    winningNumber2: number,
    winningNumber3: number
): Promise<GameDto> => {


    const request: PublishWinningNumbersRequest = {
        gameId,
        winningNumber1,
        winningNumber2,
        winningNumber3,
    };

    return await gameClient.publishWinningNumbersAndEndGame(request);

};