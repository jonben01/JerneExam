import { useState, useEffect } from "react";
import { toast } from "react-toastify";
import { publishWinningNumbers } from "../../api/adminService";
import type { GameDto } from "../../generated-ts-client";
import { getActiveGame } from "../../api/boardService";

const AdminPage = () => {
    const [activeGame, setActiveGame] = useState<GameDto | null>(null);
    const [submitting, setSubmitting] = useState(false);

    useEffect(() => {
        loadActiveGame();
    }, []);

    const loadActiveGame = async () => {
        try {
            const game = await getActiveGame();
            setActiveGame(game);
        } catch (error) {
            toast.error("Failed to load active game");
            console.error(error);
        }
    };

    const handleSubmit = async (e: React.FormEvent<HTMLFormElement>) => {
        e.preventDefault();

        const { winningNumber1, winningNumber2, winningNumber3 } = e.currentTarget;
        if (!activeGame?.id) {
            toast.error("No active game found");
            return;
        }

        const num1 = parseInt(winningNumber1.value);
        const num2 = parseInt(winningNumber2.value);
        const num3 = parseInt(winningNumber3.value);

        try {
            setSubmitting(true);
            await publishWinningNumbers(activeGame.id, num1, num2, num3);
            toast.success("Winning numbers published successfully!");
            await loadActiveGame();
        } catch (error) {
            toast.error("Failed to publish winning numbers");
            console.error(error);
        } finally {
            setSubmitting(false);
        }
    };

    return (
        <div
            style={{
                padding: "20px",
                display: "flex",
                justifyContent: "center",
            }}
        >
            <div style={{ width: "800px", maxWidth: "100%" }}>
                <h1>Admin Panel</h1>

                <div
                    style={{
                        backgroundColor: "white",
                        padding: "30px",
                        borderRadius: "8px",
                        border: "1px solid #ddd",
                    }}
                >
                    <h2 style={{ marginTop: 0 }}>Announce Winning Numbers</h2>
                    <form onSubmit={handleSubmit}>
                        <div
                            style={{
                                display: "grid",
                                gridTemplateColumns: "repeat(3, 1fr)",
                                gap: "20px",
                                marginBottom: "30px",
                            }}
                        >
                            <div>
                                <label
                                    style={{
                                        display: "block",
                                        marginBottom: "8px",
                                        fontWeight: "bold",
                                    }}
                                >
                                    Winning Number 1
                                </label>
                                <input
                                    type="number"
                                    min="1"
                                    max="16"
                                    name="winningNumber1"
                                    required
                                    style={{
                                        width: "100%",
                                        padding: "12px",
                                        fontSize: "18px",
                                        borderRadius: "4px",
                                        border: "1px solid #ccc",
                                        boxSizing: "border-box",
                                    }}
                                    placeholder="1-16"
                                />
                            </div>
                            <div>
                                <label
                                    style={{
                                        display: "block",
                                        marginBottom: "8px",
                                        fontWeight: "bold",
                                    }}
                                >
                                    Winning Number 2
                                </label>
                                <input
                                    type="number"
                                    min="1"
                                    max="16"
                                    name="winningNumber2"
                                    required
                                    style={{
                                        width: "100%",
                                        padding: "12px",
                                        fontSize: "18px",
                                        borderRadius: "4px",
                                        border: "1px solid #ccc",
                                        boxSizing: "border-box",
                                    }}
                                    placeholder="1-16"
                                />
                            </div>
                            <div>
                                <label
                                    style={{
                                        display: "block",
                                        marginBottom: "8px",
                                        fontWeight: "bold",
                                    }}
                                >
                                    Winning Number 3
                                </label>
                                <input
                                    type="number"
                                    min="1"
                                    max="16"
                                    name="winningNumber3"
                                    required
                                    style={{
                                        width: "100%",
                                        padding: "12px",
                                        fontSize: "18px",
                                        borderRadius: "4px",
                                        border: "1px solid #ccc",
                                        boxSizing: "border-box",
                                    }}
                                    placeholder="1-16"
                                />
                            </div>
                        </div>

                        <button
                            type="submit"
                            disabled={submitting}
                            style={{
                                width: "100%",
                                padding: "15px",
                                fontSize: "18px",
                                fontWeight: "bold",
                                backgroundColor: submitting ? "#ccc" : "#4CAF50",
                                color: "white",
                                border: "none",
                                borderRadius: "4px",
                                cursor: submitting ? "not-allowed" : "pointer",
                            }}
                        >
                            {submitting
                                ? "Publishing..."
                                : "Publish Winning Numbers & End Game"}
                        </button>
                    </form>
                </div>
            </div>
        </div>
    );
};

export default AdminPage;

