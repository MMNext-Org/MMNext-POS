import { z } from 'zod';

export interface Unit {
  id: number;
  name: string;
  abbreviation: string;
  baseValue: number; // how many of this unit make a base unit (e.g., 1 kg = 1000 g)
}

export interface Ingredient {
  id: number;
  name: string;
  category: string;
  unitId: number;
  baseQuantity: number; // quantity in base units
}

export const IngredientSchema = z.object({
  id: z.number().int().positive(),
  name: z.string().min(1),
  category: z.string().min(1),
  unitId: z.number().int().positive(),
  baseQuantity: z.number().int().nonnegative(),
}

export type IngredientDTO = z.infer<typeof IngredientSchema>;