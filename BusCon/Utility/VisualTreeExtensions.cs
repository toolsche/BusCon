using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace BusCon.Utility
{
    public static class VisualTreeExtensions
	{
		#region Visual Tree
		#region Children
		/// <summary>
		/// Finds all children of type T in the VisualTree beneath this <see cref="DependencyObject"/>.
		/// </summary>
		/// <remarks>
		/// If you are using a .NET language which does not support default values for parameters you can either use the
		/// fallback method without parameters or provide the following default values:
		/// </remarks>
		/// <example>
		/// To find all GroupBoxes that are direct or indirect children of a Grid g use
		/// <c>var groupBoxes = g.FindVisualChildren&lt;GroupBox&gt;();</c>To find all GroupBoxes that are direct children of a
		/// Grid g use
		/// <c>var groupBoxes = g.FindVisualChildren&lt;GroupBox&gt;(1);</c>To find all visual elements in a Grid g that have a
		/// width of 300+ pixels
		/// <c>var visuals = g.FindVisualChildren&lt;FrameworkElement&gt;(additionConstraint: (x) =&gt; x.ActualWidth &gt;= 300);</c>
		/// </example>
		/// <typeparam name="T">Type that should be searched</typeparam>
		/// <param name="searchRoot">The starting node for the DFS</param>
		/// <param name="depth">Search depth, negative values will be ignored and the entire tree will be searched</param>
		/// <param name="additionConstraint">Specify criteria for the addition to the set of results</param>
		/// <param name="recursionConstraint">Lets you specify a function that decides if the next recursive step will be
		/// performed</param>
		/// <returns>A list of visual elements of type T</returns>
		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject searchRoot,
															int depth = -1,
															Func<T, bool> additionConstraint = null,
															Func<DependencyObject, bool> recursionConstraint = null)
															where T : DependencyObject
		{
			// let's use the new level parameter to implement the old behavior
			if (depth > 0)
			{
				if (recursionConstraint == null)
				{
					return searchRoot.FindVisualChildren(additionConstraint, (child, level) => level <= depth, 0);
				}
				else
				{
					return searchRoot.FindVisualChildren(additionConstraint, (child, level) => (level <= depth && recursionConstraint(child)), 0);
				}
			}
			return searchRoot.FindVisualChildren(additionConstraint, (child, level) => recursionConstraint(child), 0);
		}
		
		/// <summary>
		/// Finds all children of type T in the VisualTree beneath this <see cref="DependencyObject"/>.
		/// </summary>
		/// <typeparam name="T">Type that should be searched</typeparam>
		/// <param name="searchRoot">The starting node for the DFS</param>
		/// <param name="additionConstraint">Specify criteria for the addition to the set of results</param>
		/// <param name="recursionConstraint">Lets you specify a function that decides if the next recursive step will be
		/// performed. The first function parameter is the child node (<see cref="DependencyObject"/>) while the second one
		/// represents the depth of the child node.</param>
		/// <returns>A list of visual elements of type T</returns>
		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject searchRoot,
															Func<T, bool> additionConstraint,
															Func<DependencyObject, int, bool> recursionConstraint)
															where T : DependencyObject
		{
			return searchRoot.FindVisualChildren<T>(additionConstraint, recursionConstraint, 0);
		}
		
		/// <summary>
		/// Finds all children of type T in the VisualTree beneath this <see cref="DependencyObject"/>.
		/// </summary>
		/// <typeparam name="T">Type that should be searched</typeparam>
		/// <param name="searchRoot">The starting node for the DFS</param>
		/// <param name="additionConstraint">Specify criteria for the addition to the set of results</param>
		/// <returns>A list of visual elements of type T</returns>
		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject searchRoot, Func<T, bool> additionConstraint) where T : DependencyObject
		{
			return searchRoot.FindVisualChildren<T>(additionConstraint, null, 0);
		}
		
		/// <summary>
		/// Finds all children of type T in the VisualTree beneath this <see cref="DependencyObject"/>.
		/// </summary>
		/// <typeparam name="T">Type that should be searched</typeparam>
		/// <param name="searchRoot">The starting node for the DFS</param>
		/// <param name="recursionConstraint">Lets you specify a function that decides if the next recursive step will be
		/// performed. The first function parameter is the child node (<see cref="DependencyObject"/>) while the second one
		/// represents the depth of the child node.</param>
		/// <returns>A list of visual elements of type T</returns>
		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject searchRoot, Func<DependencyObject, int, bool> recursionConstraint) where T : DependencyObject
		{
			return searchRoot.FindVisualChildren<T>(null, recursionConstraint, 0);
		}
		
		/// <summary>
		/// Finds all children of type T in the VisualTree beneath this <see cref="DependencyObject"/>.
		/// </summary>
		/// <typeparam name="T">Type that should be searched</typeparam>
		/// <param name="searchRoot">The starting node for the DFS</param>
		/// <returns>A list of visual elements of type T</returns>
		public static IEnumerable<T> FindVisualChildren<T>(this DependencyObject searchRoot) where T : DependencyObject
		{
			return searchRoot.FindVisualChildren<T>(null, null, 0);
		}
		
		/// <summary>
		/// Finds all children of type T in the VisualTree beneath this <see cref="DependencyObject"/>.
		/// </summary>
		/// <remarks>
		/// </remarks>
		/// <example>
		/// To find all GroupBoxes that are direct or indirect children of a Grid g use
		/// <c>var groupBoxes = g.FindVisualChildren&lt;GroupBox&gt;();</c>To find all GroupBoxes that are direct children of a
		/// Grid g use
		/// <c>var groupBoxes = g.FindVisualChildren&lt;GroupBox&gt;(1);</c>To find all visual elements in a Grid g that have a
		/// width of 300+ pixels
		/// <c>var visuals = g.FindVisualChildren&lt;FrameworkElement&gt;(additionConstraint: (x) =&gt; x.ActualWidth &gt;= 300);</c>
		/// </example>
		/// <typeparam name="T">Type that should be searched</typeparam>
		/// <param name="searchRoot">The starting node for the DFS</param>
		/// <param name="additionConstraint">Specify criteria for the addition to the set of results</param>
		/// <param name="recursionConstraint">Lets you specify a function that decides if the next recursive step will be
		/// performed</param>
		/// <param name="depth">Current depth of the recursion. Use the value 0 when calling this method.</param>
		/// <returns>A list of visual elements of type T</returns>
		private static IEnumerable<T> FindVisualChildren<T>(this DependencyObject searchRoot,
															Func<T, bool> additionConstraint,
															Func<DependencyObject, int, bool> recursionConstraint,
															int depth)
															where T : DependencyObject
		{
			// Recursion constraint will be evaluated for each child of this node, so increase the depth by one
			depth++;
			// yes, someone might try the following: (null as DependencyObject).FindVisualChildren<DependencyObject>();
			if (searchRoot != null)
			{
				// for all visual children
				for (int i = 0; i < VisualTreeHelper.GetChildrenCount(searchRoot); i++)
				{
					DependencyObject child = VisualTreeHelper.GetChild(searchRoot, i);
					// add to results if the child is of type T and the constraint function evaluates to true (or is not specified)
					if (child is T)
					{
						T c = (T)child;
						if (additionConstraint == null || additionConstraint(c))
						{
							yield return c;
						}
					}
					// do the recursive step if the constraint function evaluates to true (or if no such function was passed)
					if (recursionConstraint == null || recursionConstraint(child, depth))
					{
						// TODO: replace this as soon as yield! finds its way into C#
						foreach (var x in child.FindVisualChildren<T>(additionConstraint, recursionConstraint, depth))
						{
							yield return x;
						}
					}
				}
			}
		}
		#endregion
		
		#region Ancestors
		/// <summary>
		/// Finds all ancestors of a certain type which match a given condition.
		/// </summary>
		/// <typeparam name="T">The type which you are searching for.</typeparam>
		/// <param name="source">The search root</param>
		/// <param name="condition">The search condition, pass <c>null</c> in languages which do not support default arguments.</param>
		/// <returns>An enumeration of elements of type T</returns>
		public static IEnumerable<T> FindAncestors<T>(this DependencyObject source, Func<T, bool> condition) where T : DependencyObject
		{
			DependencyObject d = VisualTreeHelper.GetParent(source);
			if (d != null)
			{
				T a = d
				as T;
				if (a != null && (condition == null || condition(a)))
				{
					yield return a;
				}
				// Too bad we don't have "yield!" as in F# :)
				foreach (var x in d.FindAncestors(condition))
				{
					yield return x;
				}
			}
		}
		
		/// <summary>
		/// Finds all ancestors of a certain type which match a given condition.
		/// </summary>
		/// <typeparam name="T">The type which you are searching for.</typeparam>
		/// <param name="source">The search root</param>
		/// <returns>An enumeration of elements of type T</returns>
		public static IEnumerable<T> FindAncestors<T>(this DependencyObject source) where T : DependencyObject
		{
			return source.FindAncestors<T>(null);
		}
		#endregion
		#endregion
	}
}
