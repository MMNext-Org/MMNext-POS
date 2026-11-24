import { Router } from 'express';
import { z } from 'zod';
import { db } from '../db';
import { Ingredient, Unit } from '../models/Ingredient';
import { UnitConverter } from '../services/unit-conversion';

async function getUnit(unitId: number): Promise<Unit | null> {
  const unit = await db.get('SELECT * FROM units WHERE id = ?', [unitId]);
  return unit ? unit : null;
}

const validateBody = (schema) => (req, res, next) => {
  const result = schema.safeParse(req.body);
  if (!result.success) {
    return res.status(400).json({ error: 'Invalid request body', details: result.error.errors });
  }
  req.validated = result.data;
  next();
};

const router = Router();

// Get all ingredients (with unit names)
router.get('/ingredients', async (req, res) => {
  try {
    const ingredients = await db.all(`
      SELECT i.id, i.name, i.category, i.baseQuantity, u.name AS unitName, u.abbreviation AS unitAbbr
      FROM ingredients i
      JOIN units u ON i.unitId = u.id
      ORDER BY i.name
    `);
    res.json(ingredients);
  } catch (err) {
    console.error('Error fetching ingredients:', err);
    res.status(500).json({ error: 'Database error' });
  }
});

// Get ingredient by id
router.get('/ingredients/:id', async (req, res) => {
  const { id } = req.params;
  try {
    const ingredient = await db.get('SELECT * FROM ingredients WHERE id = ?', [id]);
    if (!ingredient) {
      return res.status(404).json({ error: 'Ingredient not found' });
    }
    const unit = await db.get('SELECT name, abbreviation FROM units WHERE id = ?', [ingredient.unitId]);
    res.json({ ...ingredient, unit });
  } catch (err) {
    console.error('Error fetching ingredient:', err);
    res.status(500).json({ error: 'Database error' });
  }
});

// Create new ingredient
router.post(
  '/ingredients',
  validateBody(z.object({
    name: z.string().min(1),
    category: z.string().min(1),
    unitId: z.number().int().positive(),
    baseQuantity: z.number().int().nonnegative(),
  })),
  async (req, res) => {
    const { name, category, unitId, baseQuantity } = req.validated;
    try {
      const unit = await getUnit(unitId);
      if (!unit) {
        return res.status(400).json({ error: `Unit with id ${unitId} not found` });
      }

      const result = await db.run(
        `INSERT INTO ingredients (name, category, unitId, baseQuantity) VALUES (?, ?, ?, ?)`,
        [name, category, unitId, baseQuantity]
      );
      const newIngredient = await db.get('SELECT * FROM ingredients WHERE id = ?', [result.lastID]);
      res.status(201).json(newIngredient);
    } catch (err) {
      console.error('Error creating ingredient:', err);
      res.status(500).json({ error: 'Database error' });
    }
  }
);

// Update ingredient
router.put(
  '/ingredients/:id',
  validateBody(z.object({
    name: z.string().min(1).optional(),
    category: z.string().min(1).optional(),
    unitId: z.number().int().positive().optional(),
    baseQuantity: z.number().int().nonnegative().optional(),
  })),
  async (req, res) => {
    const { id } = req.params;
    const { name, category, unitId, baseQuantity } = req.validated;
    try {
      const existing = await db.get('SELECT * FROM ingredients WHERE id = ?', [id]);
      if (!existing) {
        return res.status(404).json({ error: 'Ingredient not found' });
      }

      const fields = [];
      const values = [];

      if (name !== undefined) {
        fields.push('name = ?');
        values.push(name);
      }
      if (category !== undefined) {
        fields.push('category = ?');
        values.push(category);
      }
      if (unitId !== undefined) {
        const unit = await getUnit(unitId);
        if (!unit) {
          return res.status(400).json({ error: `Unit with id ${unitId} not found` });
        }
        fields.push('unitId = ?');
        values.push(unitId);
      }
      if (baseQuantity !== undefined) {
        fields.push('baseQuantity = ?');
        values.push(baseQuantity);
      }

      if (fields.length === 0) {
        return res.status(400).json({ error: 'No fields to update' });
      }

      values.push(id);
      const sql = `UPDATE ingredients SET ${fields.join(', ')} WHERE id = ?`;
      await db.run(sql, [...values, id]);
      const updated = await db.get('SELECT * FROM ingredients WHERE id = ?', [id]);
      res.json(updated);
    } catch (err) {
      console.error('Error updating ingredient:', err);
      res.status(500).json({ error: 'Database error' });
    }
  }
);

// Delete ingredient
router.delete('/ingredients/:id', async (req, res) => {
  const { id } = req.params;
  try {
    const result = await db.run('DELETE FROM ingredients WHERE id = ?', [id]);
    if (result.changes === 0) {
      return res.status(404).json({ error: 'Ingredient not found' });
    }
    res.status(204).send();
  } catch (err) {
    console.error('Error deleting ingredient:', err);
    res.status(500).json({ error: 'Database error' });
  }
});

// Convert value between units
router.post(
  '/convert',
  validateBody(z.object({
    value: z.number().positive(),
    fromUnitId: z.number().int().positive(),
    toUnitId: z.number().int().positive(),
  })),
  async (req, res) => {
    const { value, fromUnitId, toUnitId } = req.validated;
    try {
      const fromUnit = await db.get('SELECT * FROM units WHERE id = ?', [fromUnitId]);
      const toUnit = await db.get('SELECT * FROM units WHERE id = ?', [toUnitId]);
      if (!fromUnit || !toUnit) {
        return res.status(400).json({ error: 'Invalid unit ids' });
      }

      const converted = UnitConverter.convert(value, fromUnit, toUnit);
      res.json({ value: converted });
    } catch (err) {
      console.error('Error converting units:', err);
      res.status(500).json({ error: 'Conversion error' });
    }
  }
);

export default router;