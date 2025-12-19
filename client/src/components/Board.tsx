import type { BoardDto } from "../generated-ts-client";

const Board = ({ board }: { board: BoardDto }) => {
    const winningNumbers = [
        board.game?.winningNumber1,
        board.game?.winningNumber2,
        board.game?.winningNumber3,
    ];
    return (
        <tr
            style={{
                backgroundColor: board.isWinningBoard ? "#e7f4e4" : "#fff",
            }}
        >
            <td
                style={{
                    padding: "12px",
                    borderBottom: "1px solid #d0d0d0",
                    fontSize: "13px",
                    fontWeight: "600",
                }}
            >
                Week {board.game?.weekNumber ?? "N/A"}
            </td>
            <td
                style={{
                    padding: "12px",
                    borderBottom: "1px solid #d0d0d0",
                }}
            >
                <div
                    style={{
                        display: "flex",
                        gap: "4px",
                        flexWrap: "wrap",
                    }}
                >
                    {board.numbers?.map((num, idx) => (
                        <span
                            key={idx}
                            style={{
                                backgroundColor: winningNumbers.includes(num)
                                    ? "#70ad47"
                                    : "#f0f0f0",
                                border: "1px solid #d0d0d0",
                                padding: "4px 8px",
                                fontSize: "13px",
                                fontWeight: "500",
                                minWidth: "32px",
                                textAlign: "center",
                            }}
                        >
              {num}
            </span>
                    ))}
                </div>
            </td>
            <td
                style={{
                    padding: "12px",
                    borderBottom: "1px solid #d0d0d0",
                    fontSize: "13px",
                    fontWeight: "600",
                }}
            >
                {board.priceDkk} DKK
            </td>
            <td
                style={{
                    padding: "12px",
                    borderBottom: "1px solid #d0d0d0",
                    fontSize: "12px",
                    color: "#666",
                }}
            >
                {board.createdAt
                    ? new Date(board.createdAt).toLocaleDateString()
                    : "N/A"}
            </td>
            <td
                style={{
                    padding: "12px",
                    borderBottom: "1px solid #d0d0d0",
                    textAlign: "center",
                }}
            >
                {board.isWinningBoard && (
                    <span
                        style={{
                            backgroundColor: "#70ad47",
                            color: "white",
                            padding: "4px 12px",
                            fontSize: "11px",
                            fontWeight: "600",
                            display: "inline-block",
                        }}
                    >
            ★ WINNER
          </span>
                )}
            </td>
        </tr>
    );
};
export default Board;