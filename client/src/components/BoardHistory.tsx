import type { BoardDto } from "../generated-ts-client";
import Board from "./Board";

const BoardHistory = ({
                          boards,
                          title,
                      }: {
    boards: BoardDto[];
    title: string;
}) => {
    return (
        <div style={{ marginTop: "40px" }}>
            <div
                style={{
                    display: "flex",
                    justifyContent: "space-between",
                    alignItems: "center",
                    marginBottom: "10px",
                }}
            >
                <h3>{title}</h3>
            </div>
            {boards.length === 0 ? (
                <div style={{ color: "#666", padding: "20px" }}>No boards yet</div>
            ) : (
                <div style={{ overflowX: "auto" }}>
                    <table
                        style={{
                            width: "100%",
                            borderCollapse: "collapse",
                            border: "1px solid #d0d0d0",
                            backgroundColor: "#fff",
                            fontFamily: "'Segoe UI', Tahoma, Geneva, Verdana, sans-serif",
                            marginTop: "20px",
                        }}
                    >
                        <thead>
                        <tr style={{ backgroundColor: "#4472c4", color: "white" }}>
                            <th
                                style={{
                                    padding: "12px",
                                    textAlign: "left",
                                    fontSize: "13px",
                                    fontWeight: "600",
                                    borderBottom: "2px solid #d0d0d0",
                                }}
                            >
                                Week number
                            </th>
                            <th
                                style={{
                                    padding: "12px",
                                    textAlign: "left",
                                    fontSize: "13px",
                                    fontWeight: "600",
                                    borderBottom: "2px solid #d0d0d0",
                                }}
                            >
                                Numbers
                            </th>
                            <th
                                style={{
                                    padding: "12px",
                                    textAlign: "left",
                                    fontSize: "13px",
                                    fontWeight: "600",
                                    borderBottom: "2px solid #d0d0d0",
                                }}
                            >
                                Price
                            </th>
                            <th
                                style={{
                                    padding: "12px",
                                    textAlign: "left",
                                    fontSize: "13px",
                                    fontWeight: "600",
                                    borderBottom: "2px solid #d0d0d0",
                                }}
                            >
                                Created
                            </th>
                            <th
                                style={{
                                    padding: "12px",
                                    textAlign: "center",
                                    fontSize: "13px",
                                    fontWeight: "600",
                                    borderBottom: "2px solid #d0d0d0",
                                }}
                            >
                                Status
                            </th>
                        </tr>
                        </thead>
                        <tbody>
                        {boards.map((board) => (
                            <Board key={board.id} board={board} />
                        ))}
                        </tbody>
                    </table>
                </div>
            )}
        </div>
    );
};

export default BoardHistory;