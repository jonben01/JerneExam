import { useState, useEffect } from "react";
import { toast } from "react-toastify";
import {
    ApiException,
    type PendingDepositsListItemDto,
} from "../../../generated-ts-client";
import {
    getPendingDeposits,
    approveDeposit,
    rejectDeposit,
} from "../../../api/transactionService";
import TransactionRequest from "../../../components/TransactionRequest";

const TransactionsPage = () => {
    const [transactions, setTransactions] = useState<
        PendingDepositsListItemDto[]
    >([]);
    const [loading, setLoading] = useState(true);

    const loadDeposits = async () => {
        try {
            setLoading(true);
            const data = await getPendingDeposits();
            setTransactions(data);
        } catch (error: unknown) {
            const message =
                error instanceof ApiException
                    ? error.message
                    : "Failed to load pending deposits";
            toast.error(message);
        } finally {
            setLoading(false);
        }
    };

    useEffect(() => {
        void loadDeposits();
    }, []);

    const handleApprove = async (transactionId: string | undefined) => {
        if (!transactionId) {
            toast.error("Invalid transaction ID");
            return;
        }
        try {
            await approveDeposit(transactionId);
            await loadDeposits();
            toast.success(`Approved deposit.`);
        } catch (error) {
            toast.error("Failed to approve deposit");
            console.error(error);
        }
    };

    const handleReject = async (transactionId: string | undefined) => {
        if (!transactionId) {
            toast.error("Invalid transaction ID");
            return;
        }
        try {
            await rejectDeposit(transactionId);
            await loadDeposits();
            toast.error(`Rejected deposit.`);
        } catch (error) {
            toast.error("Failed to reject deposit");
            console.error(error);
        }
    };

    return (
        <div style={{ width: "800px", maxWidth: "100%" }}>
            <h1>Manage transactions</h1>
            <h2>Pending Deposits ({transactions.length})</h2>

            {loading ? (
                <p style={{ color: "#666", fontStyle: "italic" }}>Loading...</p>
            ) : transactions.length === 0 ? (
                <p style={{ color: "#666", fontStyle: "italic" }}>
                    No pending deposits
                </p>
            ) : (
                <div style={{ display: "flex", flexDirection: "column", gap: "12px" }}>
                    {transactions.map((transaction) => (
                        <TransactionRequest
                            key={transaction.transactionId}
                            transaction={transaction}
                            onApprove={handleApprove}
                            onReject={handleReject}
                        />
                    ))}
                </div>
            )}
        </div>
    );
};

export default TransactionsPage;