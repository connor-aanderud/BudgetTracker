import { useCallback, useState, type DragEvent } from 'react';
import { Card } from '@/components/ui/card';
import { Button } from '@/components/ui/button';

interface FileDropZoneProps {
  onFileSelected: (file: File) => void;
  uploading: boolean;
}

export default function FileDropZone({ onFileSelected, uploading }: FileDropZoneProps) {
  const [dragOver, setDragOver] = useState(false);

  const handleDrop = useCallback(
    (e: DragEvent<HTMLDivElement>) => {
      e.preventDefault();
      setDragOver(false);
      const file = e.dataTransfer.files[0];
      if (file && file.name.endsWith('.csv')) {
        onFileSelected(file);
      }
    },
    [onFileSelected]
  );

  const handleDragOver = useCallback((e: DragEvent<HTMLDivElement>) => {
    e.preventDefault();
    setDragOver(true);
  }, []);

  const handleDragLeave = useCallback(() => {
    setDragOver(false);
  }, []);

  const handleFileInput = useCallback(
    (e: React.ChangeEvent<HTMLInputElement>) => {
      const file = e.target.files?.[0];
      if (file) {
        onFileSelected(file);
      }
      e.target.value = '';
    },
    [onFileSelected]
  );

  return (
    <Card
      className={`border-2 border-dashed p-12 text-center transition-colors ${
        dragOver
          ? 'border-primary bg-primary/5'
          : 'border-muted-foreground/25 hover:border-muted-foreground/50'
      }`}
      onDrop={handleDrop}
      onDragOver={handleDragOver}
      onDragLeave={handleDragLeave}
    >
      <div className="flex flex-col items-center gap-4">
        <div className="text-4xl text-muted-foreground">
          {uploading ? '...' : '+'}
        </div>
        <div>
          <p className="text-lg font-medium">
            {uploading ? 'Uploading...' : 'Drop your CSV file here'}
          </p>
          <p className="text-sm text-muted-foreground mt-1">
            Supports Chase, Mint, and generic CSV formats
          </p>
        </div>
        {!uploading && (
          <label>
            <Button variant="outline" asChild>
              <span>Browse files</span>
            </Button>
            <input
              type="file"
              accept=".csv"
              className="hidden"
              onChange={handleFileInput}
            />
          </label>
        )}
      </div>
    </Card>
  );
}