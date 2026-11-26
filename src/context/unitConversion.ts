export interface IngredientInfo {
  unit: string;
  conversion_rate: number;
}

export const convertQuantity = (quantity: number, _unit: string, conversionRate: number): number => {
  // If unit is already the base unit, return quantity as is
  // Otherwise, convert using conversion_rate
  // For example: 1 kg = 2 units, so if we have 3 kg with conversion_rate 2, 
  // we convert to 3 * 2 = 6 base units
  return quantity * conversionRate;
};