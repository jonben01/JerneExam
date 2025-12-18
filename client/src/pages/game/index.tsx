import { useEffect, useState } from "react";
import { getActiveGame, purchaseBoard } from "../../api/boardService";
import Button from "../../components/Button";
import type { GameDto } from "../../generated-ts-client";

const BUTTON_AMOUNT = 16;

const GamePage = () => {
    const [selectedButtons, setSelectedButtons] = useState<number[]>([]);
    const [error, setError] = useState<string>("");
    const [game, setGame] = useState<GameDto | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [isSubmitting, setIsSubmitting] = useState<boolean>(false);

    useEffect(() => {
        const fetchActiveGame = async () => {
            try {
                setLoading(true);
                const activeGame = await getActiveGame();
                setGame(activeGame);
                setError("");
            } catch (err) {
                setError("Failed to load active game. Please try again.");
                console.error(err);
            } finally {
                setLoading(false);
            }
        };

        fetchActiveGame();
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

        try {
            setIsSubmitting(true);
            setError("");
            await purchaseBoard(selectedButtons, game.id!);
            setSelectedButtons([]);
            alert("Board purchased successfully!");
        } catch (err) {
            setError("Failed to purchase board. Please try again.");
        } finally {
            setIsSubmitting(false);
        }
    };

    return (
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
                </div>
            )}
            <div
                style={{
                    display: "grid",
                    gridTemplateColumns: "repeat(4, 1fr)",
                    gap: "10px",
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
            <button
                onClick={handleSubmit}
                disabled={isSubmitting || !game || selectedButtons.length === 0}
                style={{ marginTop: "20px" }}
            >
                {isSubmitting ? "Submitting..." : "Send"}
            </button>
        </div>
    );
};

export default GamePage;