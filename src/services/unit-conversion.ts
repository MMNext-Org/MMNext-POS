import { Unit } from '../models/Unit';

export class UnitConverter {
  /**
   * Convert a numeric value from one unit to another.
   * @param value - the amount in the source unit
   * @param fromUnit - the unit to convert from
   * @param toUnit - the unit to convert to
   * @returns the converted value in the target unit
   */
  static convert(value: number, fromUnit: Unit, toUnit: Unit): number {
    if (fromUnit.id === toUnit.id) {
      return value;
    }
    // Convert to base unit first
    const baseValue = value * fromUnit.baseValue;
    // Then convert to target unit
    return baseValue / toUnit.baseValue;
  }
}