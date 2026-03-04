import { useCallback } from 'react';
import { useAppDispatch, useAppSelector } from '@/store/hooks';
import {
  uploadFile,
  confirmImport,
  clearParseResult,
} from './statementsSlice';
import FileDropZone from './components/FileDropZone';
import ImportPreview from './components/ImportPreview';
import ImportHistory from './components/ImportHistory';

export default function UploadPage() {
  const dispatch = useAppDispatch();
  const {
    parseResult,
    uploading,
    uploadError,
    confirming,
    confirmError,
    confirmMessage,
  } = useAppSelector((state) => state.statements);

  const handleFileSelected = useCallback(
    (file: File) => {
      dispatch(uploadFile(file));
    },
    [dispatch]
  );

  const handleConfirm = useCallback(() => {
    if (parseResult) {
      dispatch(confirmImport(parseResult));
    }
  }, [dispatch, parseResult]);

  const handleCancel = useCallback(() => {
    dispatch(clearParseResult());
  }, [dispatch]);

  return (
    <div className="space-y-8">
      <div>
        <h2 className="text-2xl font-bold mb-1">Upload Statement</h2>
        <p className="text-muted-foreground">
          Import transactions from a CSV file
        </p>
      </div>

      {confirmMessage && (
        <div className="rounded-md bg-green-500/10 border border-green-500/30 p-4 text-sm text-green-700 dark:text-green-400">
          {confirmMessage}
        </div>
      )}

      {(uploadError || confirmError) && (
        <div className="rounded-md bg-destructive/10 border border-destructive/30 p-4 text-sm text-destructive">
          {uploadError || confirmError}
        </div>
      )}

      {!parseResult && (
        <FileDropZone
          onFileSelected={handleFileSelected}
          uploading={uploading}
        />
      )}

      {parseResult && (
        <ImportPreview
          parseResult={parseResult}
          onConfirm={handleConfirm}
          onCancel={handleCancel}
          confirming={confirming}
        />
      )}

      <div>
        <h3 className="text-lg font-semibold mb-4">Import History</h3>
        <ImportHistory />
      </div>
    </div>
  );
}