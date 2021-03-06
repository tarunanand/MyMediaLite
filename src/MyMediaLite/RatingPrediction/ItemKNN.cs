// Copyright (C) 2010, 2011, 2012 Zeno Gantner
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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using MyMediaLite.DataType;
using MyMediaLite.Data;
using MyMediaLite.Util;

namespace MyMediaLite.RatingPrediction
{
	/// <summary>Weighted item-based kNN</summary>
	public abstract class ItemKNN : KNN, IItemSimilarityProvider
	{
		/// <summary>Matrix indicating which item was rated by which user</summary>
		protected SparseBooleanMatrix data_item;

		///
		public override IRatings Ratings
		{
			set {
				base.Ratings = value;

				data_item = new SparseBooleanMatrix();
				for (int index = 0; index < ratings.Count; index++)
					data_item[ratings.Items[index], ratings.Users[index]] = true;
			}
		}

		/// <summary>Get positively correlated entities</summary>
		protected Func<int, IList<int>> GetPositivelyCorrelatedEntities;

		/// <summary>Predict the rating of a given user for a given item</summary>
		/// <remarks>
		/// If the user or the item are not known to the recommender, a suitable average is returned.
		/// To avoid this behavior for unknown entities, use CanPredict() to check before.
		/// </remarks>
		/// <param name="user_id">the user ID</param>
		/// <param name="item_id">the item ID</param>
		/// <returns>the predicted rating</returns>
		public override float Predict(int user_id, int item_id)
		{
			if ((user_id > MaxUserID) || (item_id > correlation.NumberOfRows - 1))
				return baseline_predictor.Predict(user_id, item_id);

			IList<int> relevant_items = GetPositivelyCorrelatedEntities(item_id);

			double sum = 0;
			double weight_sum = 0;
			uint neighbors = K;
			foreach (int item_id2 in relevant_items)
				if (data_item[item_id2, user_id])
				{
					float rating  = ratings.Get(user_id, item_id2, ratings.ByUser[user_id]);
					double weight = correlation[item_id, item_id2];
					weight_sum += weight;
					sum += weight * (rating - baseline_predictor.Predict(user_id, item_id2));

					if (--neighbors == 0)
						break;
				}

			float result = baseline_predictor.Predict(user_id, item_id);
			if (weight_sum != 0)
				result += (float) (sum / weight_sum);

			if (result > MaxRating)
				result = MaxRating;
			if (result < MinRating)
				result = MinRating;
			return result;
		}

		/// <summary>Retrain model for a given item</summary>
		/// <param name='item_id'>the item ID</param>
		abstract protected void RetrainItem(int item_id);

		///
		public override void AddRatings(IRatings ratings)
		{
			baseline_predictor.AddRatings(ratings);
			for (int index = 0; index < ratings.Count; index++)
				data_item[ratings.Items[index], ratings.Users[index]] = true;
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		public override void UpdateRatings(IRatings ratings)
		{
			baseline_predictor.UpdateRatings(ratings);
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		public override void RemoveRatings(IDataSet ratings)
		{
			baseline_predictor.RemoveRatings(ratings);
			for (int index = 0; index < ratings.Count; index++)
				data_item[ratings.Items[index], ratings.Users[index]] = false;
			foreach (int item_id in ratings.AllItems)
				RetrainItem(item_id);
		}

		///
		protected override void AddItem(int item_id)
		{
			correlation.AddEntity(item_id);
		}

		///
		public float GetItemSimilarity(int item_id1, int item_id2)
		{
			return correlation[item_id1, item_id2];
		}

		///
		public IList<int> GetMostSimilarItems(int item_id, uint n = 10)
		{
			return correlation.GetNearestNeighbors(item_id, n);
		}

		///
		public override void LoadModel(string filename)
		{
			base.LoadModel(filename);
			this.GetPositivelyCorrelatedEntities = Utils.Memoize<int, IList<int>>(correlation.GetPositivelyCorrelatedEntities);
		}
	}
}