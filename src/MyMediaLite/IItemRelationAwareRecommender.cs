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
//  You should have received a copy of the GNU General Public License
//  along with MyMediaLite.  If not, see <http://www.gnu.org/licenses/>.

using MyMediaLite.data_type;

namespace MyMediaLite
{
	/// <summary>Interface for recommender engines that take a binary relation over items into account</summary>
	public interface IItemRelationAwareRecommender : IRecommenderEngine
	{
		/// <summary>The binary item relation</summary>
		SparseBooleanMatrix ItemRelation { get; set; }

		/// <summary>Number of items</summary>
	    int NumItems { get; set; }
	}
}