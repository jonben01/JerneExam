import type { PendingDepositsListItemDto } from "../generated-ts-client";

interface TransactionRequestProps {
    transaction: PendingDepositsListItemDto;
    onApprove: (transactionId: string | undefined) => void;
    onReject: (transactionId: string | undefined) => void;
}

const TransactionRequest = ({
                                transaction,
                                onApprove,
                                onReject,
                            }: TransactionRequestProps) => {

    const submittedText = transaction.createdAt
        ? new Date(transaction.createdAt).toLocaleDateString()
        : "_";

    return (
        <div
            key={transaction.transactionId}
            style={{
                border: "1px solid #ddd",
                borderRadius: "8px",
                padding: "16px",
                display: "flex",
                alignItems: "center",
                backgroundColor: "#f9f9f9",
                justifyContent: "space-between",
                width: "100%",
            }}
        >
            <div style={{ display: "flex", flexDirection: "column", gap: "4px" }}>
                <div
                    style={{
                        fontWeight: "bold",
                        fontSize: "16px",
                    }}
                >
                    {transaction.playerFullName}
                </div>
                <div style={{ color: "#666", fontSize: "14px" }}>
                    Amount: <strong>{transaction.amountDkk} kr</strong>
                </div>
            </div>

            <div>
                <div style={{ color: "#666", fontSize: "14px" }}>
                    Mobilepay ref: {transaction.mobilePayReference}
                </div>
                <div style={{ color: "#999", fontSize: "12px" }}>
                    Submitted: {submittedText}
                </div>
            </div>

            <div style={{ display: "flex", gap: "8px" }}>
                <button
                    onClick={() => onApprove(transaction.transactionId)}
                    style={{
                        padding: "8px 16px",
                        backgroundColor: "#4CAF50",
                        color: "white",
                        border: "none",
                        borderRadius: "4px",
                        cursor: "pointer",
                        fontWeight: "bold",
                    }}
                    onMouseOver={(e) =>
                        (e.currentTarget.style.backgroundColor = "#45a049")
                    }
                    onMouseOut={(e) =>
                        (e.currentTarget.style.backgroundColor = "#4CAF50")
                    }
                >
                    Approve
                </button>
                <button
                    onClick={() => onReject(transaction.transactionId)}
                    style={{
                        padding: "8px 16px",
                        backgroundColor: "#f44336",
                        color: "white",
                        border: "none",
                        borderRadius: "4px",
                        cursor: "pointer",
                        fontWeight: "bold",
                    }}
                    onMouseOver={(e) =>
                        (e.currentTarget.style.backgroundColor = "#da190b")
                    }
                    onMouseOut={(e) =>
                        (e.currentTarget.style.backgroundColor = "#f44336")
                    }
                >
                    Reject
                </button>
            </div>
        </div>
    );
};

export default TransactionRequest;