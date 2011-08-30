using System;
using System.Collections.Generic;
using Grasshopper.Kernel.Data;

namespace gHowl.Xml {

	/// <summary>
	/// Describes the relationships between two paths
	/// </summary>
	/// <remarks>
	/// Relationship described is always A to B. Note that any relationship that is 
	/// not an exact match will be marked with dissimilar another more specific relationship.
	/// </remarks>
	/// <example>
	/// {0;0} -> {1;2;3}		= Dissimilar
	/// {0;0} -> {0;1}			= Sibling*
	/// {0;1;0;0} -> {0;2;0;0}	= Cousin*
	/// {0;0} -> {0;0;0}		= Parent *
	/// {0;0} -> {0;0;0;0}		= Ancestor *
	/// {0;0;0} -> {0;0}		= Child*
	/// {0;0;0;0} -> {0;0}		= Descendent*
	///						* will also be marked with Dissimilar
	/// </example>
	[Flags]
	public enum PathRelationship {
		/// <summary>
		/// Paths are not a match.  Paths may be dissimilar, yet have another relationship.
		/// </summary>
		/// <example>{0;1} <> {0;2}</example>
		Dissimilar = 1,
		/// <summary>
		/// The two paths are siblings. The both have the same parent.
		/// </summary>
		/// <example>{0:0} -> {0;1}</example>
		/// <remarks>
		/// For paths to be siblings, they have to be the same length (i.e. of the same generation). 
		/// Paths are not siblings if they have common nodes in earlier ancestors.
		/// The paths {0;1;0;0} and {0;2;0;0} would not be considered siblings.
		/// </remarks>
		Sibling = 2,
		/// <summary>
		/// There is a common node somewhere between the two paths.
		/// </summary>
		/// <remarks>
		/// Since most paths start with a common trunk of {0}, it is highly likely that almost all paths are cousins.
		/// However, a relationship of Cousin does not necessarily indicate any distinct or meaningful level of commonality.
		/// Therefore, the relationship of Cousin will be either rarely used or incredibly prolific.
		/// Due to the vagueness of the relationship, it cannot be depended on, whether present or not.
		/// </remarks>
		Cousin = 4,
		/// <summary>
		/// Being a parent requires that path B is the next generation from path A.
		/// </summary>
		/// <example>{0;1} -> {0;1;0}</example>
		Parent = 8,
		/// <summary>
		/// Ancestors are where the paths are similar, but path A precedes path B by two or more generations.
		/// </summary>
		/// <example>{0;0} -> {0;0;0;1} </example>
		Ancestor = 16,
		/// <summary>
		/// Being a child requires that path A is the next generation from path B.
		/// </summary>
		/// <example>{0;1;0} -> {0;1}</example>
		Child = 32,
		/// <summary>
		/// Descendents are where the paths are similar, but path B precedes path A by two or more generations.
		/// </summary>
		/// <example>{0;1;0;0} -> {0;1}</example>
		Descendent = 64,
		/// <summary>
		/// Paths are an exact match
		/// </summary>
		Match = 128
	}

	/// <summary>
	/// Adds a number of extension methods that allow for comparison and querying the 
	/// relationships of paths to other paths, branches within trees to a given path,
	/// parent-child, ancestor-descendents, siblings, etc.
	/// </summary>
	public static class PathExtension {

		/// <summary>
		/// Finds the children of a given path from a collection of paths
		/// </summary>
		/// <param name="basePath">Parent path</param>
		/// <param name="paths">Collection of paths</param>
		/// <returns>
		/// A List of GH_Paths of the children.  If no children are present, a null value is returned
		/// </returns>
		public static IList<GH_Path> FindChildren(this GH_Path basePath, IList<GH_Path> paths) {
			List<GH_Path> children = new List<GH_Path>();

			foreach ( GH_Path path in paths ) {
				PathRelationship relationship = basePath.Relationship(path, 0);
				if ( relationship == (PathRelationship.Parent | PathRelationship.Dissimilar) ) {
					children.Add(path);
				}
			}

			return children.Count == 0 ? null : children;
		}

		/// <summary>
		/// Finds the direct siblings of given path from a collection of paths
		/// </summary>
		/// <param name="basePath">Path</param>
		/// <param name="paths">Collection of paths</param>
		/// <returns>
		/// A List of GH_Paths of the siblings.  If no siblings are present, a null value is returned.
		/// </returns>
		/// <remarks>
		/// Note that Grasshopper does not necessarily return a complete tree 
		/// of paths, but rather just the paths where data actually resides.
		/// Because of this, there may be "siblings" that exist within the node 
		/// structure of the tree, but are not actually represented because 
		/// there is no data within that node. This method only returns the 
		/// siblings that actually reside in the tree, and, therefore, contain data.
		/// </remarks>
		public static IList<GH_Path> FindSiblings(this GH_Path basePath, IList<GH_Path> paths) {
			List<GH_Path> siblings = new List<GH_Path>();

			foreach ( GH_Path path in paths ) {
				PathRelationship relationship = basePath.Relationship(path, 0);
				if ( relationship == (PathRelationship.Sibling | PathRelationship.Dissimilar) ) {
					siblings.Add(path);
				}
			}

			return siblings.Count == 0 ? null : siblings;
		}

		/// <summary>
		/// Finds all siblings of a given path from a collection of paths
		/// </summary>
		/// <param name="basePath">Path</param>
		/// <param name="paths">Collection of paths</param>
		/// <returns>
		/// A List of GH_Paths of the apparent siblings.  If no siblings are present, a null value is returned.
		/// </returns>
		/// <remarks>
		/// Note that Grasshopper does not necessarily return a complete tree 
		/// of paths, but rather just the paths where data actually resides.
		/// Because of this, there may be "siblings" that exist within the node 
		/// structure of the tree, but are not actually represented because 
		/// there is no data within that node. This method returns all siblings,
		/// regardless of whether the contain data or are represented in the tree.
		/// Therefore, not all siblings that are returned will contain data, 
		/// however, all siblings that do not contain data will have a future 
		/// generation that does contain data.
		/// </remarks>
		public static IList<GH_Path> FindApparentSiblings(this GH_Path basePath, IList<GH_Path> paths) {
			GH_Path parentPath = new GH_Path(basePath).CullElement();
			List<GH_Path> apparentSiblings = new List<GH_Path>();

			foreach ( GH_Path path in paths ) {
				PathRelationship relationship = Relationship(path, parentPath, 0);
				switch ( relationship ) {
					case (PathRelationship.Child | PathRelationship.Dissimilar):
						if ( basePath.Relationship(path) != PathRelationship.Match
							&& !apparentSiblings.ContainsPath(path) ) {
							apparentSiblings.Add(path);
						}

						break;
					case (PathRelationship.Descendent | PathRelationship.Dissimilar):
						GH_Path potentialSibling = new GH_Path(parentPath);
						potentialSibling = potentialSibling.AppendElement(path[parentPath.Length]);

						if ( basePath.Relationship(potentialSibling) != PathRelationship.Match
							&& !apparentSiblings.ContainsPath(potentialSibling) ) {
							apparentSiblings.Add(potentialSibling);
						}

						break;
					default:
						break;
				}
			}

			return apparentSiblings.Count == 0 ? null : apparentSiblings;
		}

		/// <summary>
		/// Compares two paths and see if they have common components
		/// </summary>
		/// <param name="basePath">The first path</param>
		/// <param name="toPath">The second path</param>
		/// <returns>A PathComparision representing the relationship between the two paths</returns>
		/// <remarks>
		/// See PathComparision for possible results and examples.
		/// </remarks>
		/// <exception cref="IndexOutOfRangeException">Supplied index is too long for one or both paths.</exception>
		public static PathRelationship Relationship(this GH_Path basePath, GH_Path toPath) {
			return basePath.Relationship(toPath, 0);
		}

		/// <summary>
		/// Compares two paths and see if they have common components
		/// </summary>
		/// <param name="basePath">The first path</param>
		/// <param name="toPath">The second path</param>
		/// <param name="index">The index to compare</param>
		/// <returns>A PathComparision representing the relationship between the two paths</returns>
		/// <remarks>
		/// See PathComparision for possible results and examples.
		/// </remarks>
		/// <exception cref="IndexOutOfRangeException">Supplied index is too long for one or both paths.</exception>
		public static PathRelationship Relationship(this GH_Path basePath, GH_Path toPath, int index) {

			int pathLengthDifference = toPath.Length - basePath.Length;
			int indexDiff = index + 1 - basePath.Length;

			#region Handle possible exceptions before additional logic
			if ( indexDiff > 0 ) {	// || (pathLengthDifference < indexDiff)) {
				System.Diagnostics.EventLog.WriteEntry("GHPathExposer", System.Environment.StackTrace);
				throw new IndexOutOfRangeException(
					string.Format(
						"Supplied index of {0} is to large for pathA.Length {1} or pathB.Length {2}.",
						index,
						basePath.Length,
						toPath.Length
					)
				);
			}
			#endregion

			if ( indexDiff == 0 && pathLengthDifference == 0 ) {	// Last element... return a value
				return basePath[index] == toPath[index] ?
					PathRelationship.Match :
					PathRelationship.Sibling | PathRelationship.Dissimilar;
			}
			else if ( indexDiff == 0 && pathLengthDifference == 1 ) {		// Last element... return a value
				return basePath[index] == toPath[index] ?
					PathRelationship.Parent | PathRelationship.Dissimilar :
					PathRelationship.Dissimilar;	// cousin could potentially be added here
			}
			else if ( indexDiff == 0 && pathLengthDifference > 1 ) {		// Last element... return a value
				return basePath[index] == toPath[index] ?
					PathRelationship.Ancestor | PathRelationship.Dissimilar :
					PathRelationship.Dissimilar;	// cousin could potentially be added here
			}
			else if ( indexDiff == 0 && pathLengthDifference == -1 ) {	// Last element... return a value
				return PathRelationship.Child | PathRelationship.Dissimilar;
			}
			else if ( indexDiff == 0 && pathLengthDifference < -1 ) {		// Last element... return a value
				return PathRelationship.Descendent | PathRelationship.Dissimilar;
			}
			else if ( pathLengthDifference >= indexDiff ) {	// not at the end and lengths are safe, so recurse
				return basePath[index] == toPath[index] ?
					basePath.Relationship(toPath, index + 1) :
					PathRelationship.Dissimilar;
			}
			else if ( pathLengthDifference < indexDiff ) {
				return PathRelationship.Descendent | PathRelationship.Dissimilar;
			}
			else {
				return PathRelationship.Dissimilar;
			}
		}

		/// <summary>
		/// Determines whether a given path as any children or descendents.
		/// </summary>
		/// <param name="basePath">The path to test against a collection.</param>
		/// <param name="paths">A collection of paths.</param>
		/// <returns></returns>
		public static bool HasDescendents(this GH_Path basePath, IList<GH_Path> paths) {
			foreach ( GH_Path path in paths ) {
				PathRelationship relationship = basePath.Relationship(path, 0);
				if ( relationship == (PathRelationship.Parent | PathRelationship.Dissimilar)
					|| relationship == (PathRelationship.Ancestor | PathRelationship.Dissimilar) ) {
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Determines whether a given path has any siblings
		/// </summary>
		/// <param name="basePath"></param>
		/// <param name="paths"></param>
		/// <returns></returns>
		public static bool HasSiblings(this GH_Path basePath, IList<GH_Path> paths) {
			foreach ( GH_Path path in paths ) {
				PathRelationship relationship = basePath.Relationship(path, 0);
				if ( relationship == (PathRelationship.Sibling | PathRelationship.Dissimilar) ) {
					return true;
				}
			}

			return false;

		}

		/// <summary>
		/// Determines whether a given path has a parent or any ancestors
		/// </summary>
		/// <param name="basePath"></param>
		/// <param name="paths"></param>
		/// <returns></returns>
		public static bool HasAncestors(this GH_Path basePath, IList<GH_Path> paths) {
			foreach ( GH_Path path in paths ) {
				PathRelationship relationship = basePath.Relationship(path, 0);
				if ( relationship == (PathRelationship.Child | PathRelationship.Dissimilar)
					|| relationship == (PathRelationship.Descendent | PathRelationship.Dissimilar) ) {
					return true;
				}
			}

			return false;
		}

	}

	/// <summary>
	/// Adds a number of extension methods for using with an IList of GH_Paths.
	/// </summary>
	public static class PathIListExtension {

		/// <summary>
		/// Determines whether an IList of paths contains the base path.
		/// </summary>
		/// <param name="paths">Collection of paths</param>
		/// <param name="basePath">Path that will be tested</param>
		/// <returns></returns>
		/// <remarks>
		/// Since IList.Contain only test to see if an instance of the path is present, this 
		/// method test to see any exact representation of the path supplied exists in the
		/// collection. Null and zero length path collections are supported.
		/// </remarks>
		public static bool ContainsPath(this IList<GH_Path> paths, GH_Path basePath) {
			if ( paths == null || paths.Count == 0 ) {
				return false;
			}

			bool isBasePresent = false;
			foreach ( GH_Path path in paths ) {
				if ( path.IsCoincident(basePath) ) {
					isBasePresent = true;
					break;
				}
			}

			return isBasePresent;
		}

	}
}
