// Copyright (C) 2012 Zeno Gantner
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
using System.Globalization;
using System.IO;
using System.Linq;
using MyMediaLite.Data;
using MyMediaLite.DataType;
using MyMediaLite.IO;

namespace MyMediaLite.ItemRecommendation
{
	/// <summary>Recommend most popular items by attribute</summary>
	/// <remarks>
	/// <para>This method is similar to the "same artist -greatest hits" baseline in the paper below.</para>
	/// <para>
	///   Literature:
	///   <list type="bullet">
	///     <item><description>
	///       Brian McFee, Thierry Bertin-Mahieux, Daniel P.W. Ellis, Gert R.G. Lanckriet:
	///       The Million Song Dataset Challenge.
	///       ADMIRE 2012.
	///       TODO: URL
	///     </description></item>
	///   </list>
	/// </para>
	/// <para>
	///   This recommender does NOT support incremental updates.
	/// </para>
	/// </remarks>
	public class MostPopularByAttributes : ItemRecommender, IItemAttributeAwareRecommender
	{
		MostPopular most_popular = new MostPopular();
		IList<IDictionary<int, int>> attribute_count_by_user;
		
		///
		public SparseBooleanMatrix ItemAttributes
		{
			get { return this.item_attributes; }
			set {
				this.item_attributes = value;
				this.NumItemAttributes = item_attributes.NumberOfColumns;
				this.MaxItemID = Math.Max(MaxItemID, item_attributes.NumberOfRows - 1);
			}
		}
		private SparseBooleanMatrix item_attributes;

		///
		public int NumItemAttributes { get; private set; }

		///
		public override void Train()
		{
			most_popular.Feedback = Feedback;
			most_popular.Train();
			
			attribute_count_by_user = new IDictionary<int, int>[MaxUserID + 1];
			for (int u = 0; u < attribute_count_by_user.Count; u++)
				attribute_count_by_user[u] = new Dictionary<int, int>();
			
			for (int index = 0; index < Feedback.Count; index++)
			{
				int user_id = Feedback.Users[index];
				int item_id = Feedback.Items[index];
				foreach (int a in item_attributes[item_id]) // TODO speed up
					if (attribute_count_by_user[user_id].ContainsKey(a))
						attribute_count_by_user[user_id][a]++;
					else
						attribute_count_by_user[user_id][a] = 1;
			}
		}

		///
		public override float Predict(int user_id, int item_id)
		{
			if (user_id > MaxUserID || item_id > MaxItemID)
				return float.MinValue;

			int result = 0;
			foreach (int a in item_attributes[item_id])
				if (attribute_count_by_user[user_id].ContainsKey(a)) // TODO speed up
					result += attribute_count_by_user[user_id][a];
			return (float) result * most_popular.Predict(user_id, item_id);
		}

		///
		public override void SaveModel(string filename)
		{
			throw new NotImplementedException();
		}

		///
		public override void LoadModel(string filename)
		{
			throw new NotImplementedException();
		}
	}
}