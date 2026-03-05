import { useState, useRef } from "react";
import { Download, Upload, AlertTriangle, Loader2 } from "lucide-react";
import apiClient from "@/services/apiClient";
import { Button } from "@/components/ui/button";
import {
  Card,
  CardContent,
  CardDescription,
  CardHeader,
  CardTitle,
} from "@/components/ui/card";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from "@/components/ui/dialog";

export default function SettingsPage() {
  const [backupLoading, setBackupLoading] = useState(false);
  const [restoreLoading, setRestoreLoading] = useState(false);
  const [selectedFile, setSelectedFile] = useState<File | null>(null);
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [message, setMessage] = useState<{
    type: "success" | "error";
    text: string;
  } | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  const handleBackup = async () => {
    setBackupLoading(true);
    setMessage(null);
    try {
      const response = await apiClient.get("/data/backup", {
        responseType: "blob",
      });

      const url = window.URL.createObjectURL(new Blob([response.data]));
      const link = document.createElement("a");
      link.href = url;
      link.download = `budget_backup_${new Date().toISOString().slice(0, 10)}.db`;
      link.click();
      window.URL.revokeObjectURL(url);
      setMessage({ type: "success", text: "Backup downloaded successfully." });
    } catch {
      setMessage({
        type: "error",
        text: "Failed to download backup. Please try again.",
      });
    } finally {
      setBackupLoading(false);
    }
  };

  const handleRestore = async () => {
    if (!selectedFile) return;
    setConfirmOpen(false);
    setRestoreLoading(true);
    setMessage(null);
    try {
      const formData = new FormData();
      formData.append("file", selectedFile);

      await apiClient.post("/data/restore", formData, {
        headers: { "Content-Type": "multipart/form-data" },
      });

      setMessage({
        type: "success",
        text: "Database restored successfully. Please refresh the page to see updated data.",
      });
      setSelectedFile(null);
      if (fileInputRef.current) {
        fileInputRef.current.value = "";
      }
    } catch {
      setMessage({
        type: "error",
        text: "Failed to restore database. Please check the file and try again.",
      });
    } finally {
      setRestoreLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h2 className="text-2xl font-bold mb-1">Settings</h2>
        <p className="text-muted-foreground">
          Manage your application data
        </p>
      </div>

      {message && (
        <div
          className={`rounded-md border p-4 text-sm ${
            message.type === "success"
              ? "bg-green-50 border-green-200 text-green-800 dark:bg-green-950 dark:border-green-800 dark:text-green-200"
              : "bg-destructive/10 border-destructive/30 text-destructive"
          }`}
        >
          {message.text}
        </div>
      )}

      <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
        {/* Backup Database */}
        <Card>
          <CardHeader>
            <CardTitle>Backup Database</CardTitle>
            <CardDescription>
              Download a copy of your database for safekeeping. You can use this
              file to restore your data later.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <Button onClick={handleBackup} disabled={backupLoading}>
              {backupLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Download className="mr-2 h-4 w-4" />
              )}
              Download Backup
            </Button>
          </CardContent>
        </Card>

        {/* Restore Database */}
        <Card>
          <CardHeader>
            <CardTitle>Restore Database</CardTitle>
            <CardDescription>
              Upload a previously saved database file to restore your data.
            </CardDescription>
          </CardHeader>
          <CardContent className="space-y-4">
            <div className="flex items-start gap-2 rounded-md bg-amber-50 border border-amber-200 p-3 text-sm text-amber-800 dark:bg-amber-950 dark:border-amber-800 dark:text-amber-200">
              <AlertTriangle className="h-4 w-4 mt-0.5 shrink-0" />
              <span>
                Restoring a database will overwrite all existing data. This
                action cannot be undone.
              </span>
            </div>
            <div className="flex items-center gap-3">
              <input
                ref={fileInputRef}
                type="file"
                accept=".db"
                onChange={(e) => setSelectedFile(e.target.files?.[0] ?? null)}
                className="text-sm file:mr-3 file:rounded-md file:border-0 file:bg-primary file:px-3 file:py-1.5 file:text-sm file:font-medium file:text-primary-foreground hover:file:bg-primary/90 file:cursor-pointer"
              />
            </div>
            <Button
              onClick={() => setConfirmOpen(true)}
              disabled={!selectedFile || restoreLoading}
              variant="destructive"
            >
              {restoreLoading ? (
                <Loader2 className="mr-2 h-4 w-4 animate-spin" />
              ) : (
                <Upload className="mr-2 h-4 w-4" />
              )}
              Restore
            </Button>
          </CardContent>
        </Card>
      </div>

      {/* Restore Confirmation Dialog */}
      <Dialog open={confirmOpen} onOpenChange={setConfirmOpen}>
        <DialogContent>
          <DialogHeader>
            <DialogTitle>Confirm Restore</DialogTitle>
            <DialogDescription>
              Are you sure you want to restore the database from{" "}
              <strong>{selectedFile?.name}</strong>? This will overwrite all
              existing data and cannot be undone.
            </DialogDescription>
          </DialogHeader>
          <DialogFooter>
            <Button variant="outline" onClick={() => setConfirmOpen(false)}>
              Cancel
            </Button>
            <Button variant="destructive" onClick={handleRestore}>
              Restore
            </Button>
          </DialogFooter>
        </DialogContent>
      </Dialog>
    </div>
  );
}
