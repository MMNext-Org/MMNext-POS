import type { Unit } from './models/Unit';

export class UnitConverter {
  static convert(value: number, fromUnit: Unit, toUnit: Unit): number {
    if (fromUnit.id === toUnit.id) {
      return value;
    }
    const baseValue = value * fromUnit.baseValue;
    return baseValue / toUnit.baseValue;
  }
}