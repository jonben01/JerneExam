import { useEffect, useMemo, useState } from "react";
import {
    getActiveGame,
    getMyBoards,
    purchaseBoard,
} from "../../api/boardService";
import Button from "../../components/Button";
import type { BoardDto, GameDto } from "../../generated-ts-client";
import { toast } from "react-toastify";
import BoardHistory from "../../components/BoardHistory";
import { getMyBalance } from "../../api/transactionService";

const getBoardCost = (numberOfSelections: number): number => {
    if (numberOfSelections <= 5) return 20;
    if (numberOfSelections === 6) return 40;
    if (numberOfSelections === 7) return 80;
    if (numberOfSelections === 8) return 160;
    return 0;
};

const BUTTON_AMOUNT = 16;

const GamePage = () => {
    const [selectedButtons, setSelectedButtons] = useState<number[]>([]);
    const [error, setError] = useState<string>("");
    const [game, setGame] = useState<GameDto | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [isSubmitting, setIsSubmitting] = useState<boolean>(false);
    const [boards, setBoards] = useState<Array<BoardDto>>([]);
    const [balance, setBalance] = useState<number>(0);

    const { currentGameBoards, oldBoards } = useMemo(() => {
        if (!game) return { currentGameBoards: [], oldBoards: boards };
        return {
            currentGameBoards: boards.filter((board) => board.gameId === game.id),
            oldBoards: boards.filter((board) => board.gameId !== game.id),
        };
    }, [boards, game]);

    const currentCost = useMemo(
        () => getBoardCost(selectedButtons.length),
        [selectedButtons.length]
    );
    const canAfford = useMemo(
        () => balance >= currentCost,
        [balance, currentCost]
    );

    const fetchBalance = async () => {
        const userBalance = await getMyBalance();
        setBalance(userBalance);
    };

    const fetchBoards = async () => {
        const data = await getMyBoards();
        setBoards(data);
    };

    useEffect(() => {
        const fetchInitialData = async () => {
            setLoading(true);
            const activeGame = await getActiveGame();
            await fetchBalance();
            await fetchBoards();
            setGame(activeGame);
            setError("");
            setLoading(false);
        };

        fetchInitialData();
    }, []);

    const handleSelectButton = (value: number) => {
        setSelectedButtons((prevSelected) => {
            if (prevSelected.includes(value)) {
                setError("");
                return prevSelected.filter((v) => v !== value);
            } else {
                if (prevSelected.length >= 8) {
                    setError("You can only select up to 8 numbers!");
                    return prevSelected;
                }
                setError("");
                return [...prevSelected, value];
            }
        });
    };

    const handleSubmit = async () => {
        if (!game) {
            setError("No active game found");
            return;
        }

        if (selectedButtons.length === 0) {
            setError("Please select at least one number");
            return;
        }

        setIsSubmitting(true);
        setError("");
        await purchaseBoard(selectedButtons, game.id!);
        setSelectedButtons([]);
        toast.success("Board purchased successfully!");
        await fetchBoards();
        await fetchBalance();
        setIsSubmitting(false);
    };

    return (
        <>
            <div>
                {loading && <div>Loading game...</div>}
                {error && (
                    <div
                        style={{
                            color: "red",
                            marginBottom: "10px",
                            padding: "10px",
                            border: "1px solid red",
                            borderRadius: "5px",
                            backgroundColor: "#ffe6e6",
                        }}
                    >
                        {error}
                    </div>
                )}
                {game && (
                    <div style={{ marginBottom: "20px" }}>
                        <h2>Game at week: {game.weekNumber}</h2>
                        <p>Selected: {selectedButtons.length} / 8 numbers</p>
                        {selectedButtons.length >= 5 && (
                            <p style={{ fontWeight: "600", margin: "8px 0" }}>
                                Cost: {currentCost} DKK
                            </p>
                        )}
                    </div>
                )}
                <div
                    style={{
                        display: "grid",
                        gridTemplateColumns: "repeat(4, 1fr)",
                        gap: "16px",
                    }}
                >
                    {Array.from({ length: BUTTON_AMOUNT }, (_, i) => {
                        const value = i + 1;
                        return (
                            <Button
                                key={i}
                                toggled={selectedButtons.includes(value)}
                                onClick={() => handleSelectButton(value)}
                            >
                                {value}
                            </Button>
                        );
                    })}
                </div>
                <div style={{ marginTop: "20px", fontSize: "16px", fontWeight: "600" }}>
                    Balance: {balance} DKK
                </div>
                {selectedButtons.length >= 5 && !canAfford && (
                    <div
                        style={{
                            color: "#d9534f",
                            marginTop: "10px",
                            padding: "10px",
                            border: "1px solid #d9534f",
                            borderRadius: "5px",
                            backgroundColor: "#f2dede",
                            fontWeight: "600",
                        }}
                    >
                        You cannot afford this board! You need {currentCost} DKK but only
                        have {balance} DKK.
                    </div>
                )}
                <button
                    onClick={handleSubmit}
                    disabled={
                        isSubmitting || !game || selectedButtons.length === 0 || !canAfford
                    }
                    style={{ marginTop: "20px" }}
                >
                    {isSubmitting ? "Submitting..." : "Send"}
                </button>
            </div>
            <BoardHistory
                boards={currentGameBoards}
                title="Boards purchased for the active game"
            />
            <BoardHistory
                boards={oldBoards}
                title="Boards purchased for previous games"
            />
        </>
    );
};

export default GamePage;