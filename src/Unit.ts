import { z } from 'zod';

export interface Unit {
  id: number;
  name: string;
  abbreviation: string;
  baseValue: number; // how many of this unit make a base unit (e.g., 1 kg = 1000 g)
}

export const UnitSchema = z.object({
  id: z.number().int().positive(),
  name: z.string().min(1),
  abbreviation: z.string().min(1),
  baseValue: z.number().int().positive(),
}

export type UnitDTO = z.infer<typeof UnitSchema>;