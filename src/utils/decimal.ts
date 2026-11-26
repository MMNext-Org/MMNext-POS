export class Decimal {
  private value: string;

  constructor(value: number | string) {
    this.value = typeof value === 'number' 
      ? value.toString() 
      : String(value);
  }

  toFloat(): number {
    return parseFloat(this.value);
  }

  toNumber(): number {
    return parseFloat(this.value);
  }

  add(other: number | string): Decimal {
    const otherValue = new Decimal(other);
    return new Decimal(this.toNumber() + otherValue.toNumber());
  }

  subtract(other: number | string): Decimal {
    const otherValue = new Decimal(other);
    return new Decimal(this.toNumber() - otherValue.toNumber());
  }

  multiply(other: number | string): Decimal {
    const otherValue = new Decimal(other);
    return new Decimal(this.toNumber() * otherValue.toNumber());
  }

  divide(other: number | string): Decimal {
    const otherValue = new Decimal(other);
    return new Decimal(this.toNumber() / otherValue.toNumber());
  }

  round(decimals: number = 2): Decimal {
    const factor = Math.pow(10, decimals);
    return new Decimal(Math.round(parseFloat(this.value) * factor) / factor);
  }

  toFixed(decimals: number = 2): string {
    return this.toNumber().toFixed(decimals);
  }

  equals(other: number | string): boolean {
    return this.value === new Decimal(other).value;
  }
}