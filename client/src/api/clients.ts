import {
    BoardClient,
    GameClient,
    PlayerClient,
    TransactionClient,
} from "../generated-ts-client";
import { API_BASE_URL } from "./authService";
import { createJwtHttp } from "./nswagJwtHttp";

const jwtHttp = createJwtHttp();

export const boardClient = new BoardClient(API_BASE_URL, jwtHttp);
export const gameClient = new GameClient(API_BASE_URL, jwtHttp);
export const playerClient = new PlayerClient(API_BASE_URL, jwtHttp);
export const transactionClient = new TransactionClient(API_BASE_URL, jwtHttp);