// Copyright (C) 2010 Zeno Gantner
// 
// This file is part of MyMediaLite.
// 
// MyMediaLite is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// MyMediaLite is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using MyMediaLite.Data;
using MyMediaLite.DataType;

namespace MyMediaLite.AttrToFactor
{
	public class MF_Item_Mapping_Optimal : MF_ItemMapping
	{
		
		/// <inheritdoc/>
		public override void LearnAttributeToFactorMapping()
		{
			attribute_to_factor = new Matrix<double>(NumItemAttributes + 1, num_factors + 1);
			MatrixUtils.InitNormal(attribute_to_factor, InitMean, InitStdev);			
			
			for (int i = 0; i < num_iter_mapping; i++)
				IterateMapping();			
		}
		
		/// <summary>One (stochastic) pass over the training data</summary>
		public override void IterateMapping()
		{
			var factor_bias_gradient         = new double[factor_bias.Length];
			var attribute_to_factor_gradient = new Matrix<double>(attribute_to_factor.dim1, attribute_to_factor.dim2);

			var item_latent_factors = new double[MaxItemID + 1][];
			var item_est_bias       = new double[MaxItemID + 1];

			double rating_range_size = MaxRating - MinRating;
			
			// 1. estimate factors for all items
			for (int i = 0; i <= MaxItemID; i++)
			{
				// estimate factors
				double[] est_factors = MapToLatentFactorSpace(i);
				Array.Copy(est_factors, item_latent_factors[i], num_factors);
				item_est_bias[i] = est_factors[num_factors];
			}
			
			// 2. compute gradients
			foreach (RatingEvent r in Ratings.All)
			{
		        double score =
					MatrixUtils.RowScalarProduct(user_factors, r.user_id, item_latent_factors[r.item_id])
					+ user_bias[r.user_id]
					+ item_est_bias[r.item_id] // estimated item bias
					+ global_bias;
				
				double sig_score = 1 / (1 + Math.Exp(-score));

                double p = MinRating + sig_score * rating_range_size;
				double err = r.rating - p;

				double gradient_common = err * sig_score * (1 - sig_score) * rating_range_size;
				
				foreach (int a in ItemAttributes[r.item_id])
				{
					for (int f = 0; f < attribute_to_factor.dim2; f++)
						attribute_to_factor_gradient[a, f] += gradient_common * user_factors[r.user_id, f] * attribute_to_factor[f, a];
				}
			}
			
			// 3. gradient descent step
		}
	}
}

