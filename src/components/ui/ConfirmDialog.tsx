import { Dialog, DialogOverlay, DialogContent, DialogTitle, Button } from '@chakra-ui/react';

type ConfirmDialogProps = {
  isOpen: boolean;
  onConfirm: () => void;
  onCancel: () => void;
  children: React.ReactNode;
};

export const ConfirmDialog = ({ isOpen, onConfirm, onCancel, children }: ConfirmDialogProps) => {
  return (
    <Dialog isOpen={isOpen} placement="center">
      <DialogOverlay />
      <DialogContent className="w-full p-3 md:p-5">
        <DialogTitle>{children}</DialogTitle>
              <Button colorScheme="red" onClick={onCancel} px-5 py-3 rounded-md>Cancel</Button>
              <Button colorScheme="teal" onClick={onConfirm} px-5 py-3 rounded-md>Confirm</Button>
      </DialogContent>
    </Dialog>
  );
};