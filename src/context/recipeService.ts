import { supabase } from '@/utils/supabase';
import type { IngredientInfo } from './unitConversion';

// Initialize Supabase client - this should be imported from your existing supabase client
// For now, we'll assume you have a supabase client available

/**
 * Get ingredients for a product variant, including unit conversion information
 * @param productVariantId - The ID of the product variant
 * @returns Array of ingredients with quantity, unit, and conversion information
 */
export const getProductVariantRecipe = async (productVariantId: string): Promise<IngredientInfo[]> => {
  // 1. Get the product ID and organization ID from the product variant
  const { data: variant, error: variantError } = await supabase
    .from('product_variants')
    .select('id, product_id, organization_id')
    .eq('id', productVariantId)
    .single();
  
  if (variantError) {
    throw variantError;
  }
  
  const productId = variant.product_id;
  const organizationId = variant.organization_id;
  
  // 2. Get recipes for this product variant
  // Check if there's a direct relationship between product variants and recipes
  const { data: variantRecipes } = await supabase
    .from('recipes')
    .select('id')
    .eq('organization_id', organizationId)
    .eq('product_variant_id', productVariantId); // Assuming direct relationship as per spec
  
  let recipes = variantRecipes || []; // Use variant-specific recipes if they exist
  
  // If no variant-specific recipes found, fall back to product-level recipes
  if (variantRecipes?.length === 0) {
    const { data: productRecipes, error: productRecipesError } = await supabase
      .from('recipes')
      .select('id')
      .eq('organization_id', organizationId)
      .eq('product_id', productId); // Fall back to product-level recipes
      
    if (productRecipesError) {
      throw new Error('Could not find recipes for product variant or product.');
    }
    
    recipes = productRecipes;
  }
  
  // 3. Get all ingredients from these recipes
  const ingredientIds = new Set<string>();
  recipes.forEach(recipe => {
    ingredientIds.add(recipe.id);
  });
  
  if (ingredientIds.size === 0) {
    return []; // No recipes found
  }
  
  const { data: recipeItems, error: itemsError } = await supabase
    .from('recipe_items')
    .select('ingredient_id, quantity, unit_id')
    .in('recipe_id', Array.from(ingredientIds))
    .order('ingredient_id');
  
  if (itemsError) {
    throw itemsError;
  }
  
  // 4. Get ingredient information for each ingredient and convert quantities
  const ingredientResults = await Promise.all(
    recipeItems.map(async (item) => {
      const { data: ingredient, error: ingredientError } = await supabase
        .from('ingredients')
        .select('id, unit, conversion_rate')
        .eq('id', item.ingredient_id)
        .single();
      
      if (ingredientError) {
        throw ingredientError;
      }
      
      // Convert quantity to base units
      const convertedQuantity = convertQuantity(
        item.quantity, 
        ingredient.unit, 
        ingredient.conversion_rate
      );
      
      return {
        quantity: convertedQuantity,
        unit: ingredient.unit,
        conversion_rate: ingredient.conversion_rate
      };
    })
  );
  
  return ingredientResults;
};

/**
 * Helper function to convert quantity to base units
 * @param quantity - Original quantity
 * @param unit - Unit type (e.g., 'kg', 'g', 'pcs')
 * @param conversionRate - How many base units this unit represents
 * @returns Quantity in base units
 */
export const convertQuantity = (quantity: number, _unit: string, conversionRate: number): number => {
  // If unit is already the base unit, return quantity as is
  // Otherwise, convert using conversion_rate
  // For example: 1 kg = 2 units, so if we have 3 kg with conversion_rate 2, 
  // we convert to 3 * 2 = 6 base units
  return quantity * conversionRate;
};