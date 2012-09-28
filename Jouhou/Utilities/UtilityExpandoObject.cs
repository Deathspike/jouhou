// ======================================================================
// This source code form is subject to the terms of the Mozilla Public
// License, version 2.0. If a copy of the MPL was not distributed with 
// this file, you can obtain one at http://mozilla.org/MPL/2.0/.
// ======================================================================
using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;

namespace Jouhou {
	/// <summary>
	/// Represents a case-insensitive ExpandoObject.
	/// </summary>
	public class UtilityExpandoObject : DynamicObject, IDictionary<string, object> {
		/// <summary>
		/// Contains the collection.
		/// </summary>
		private IDictionary<string, object> _Dictionary;

		/// <summary>
		/// Initialize a new instance of the UtilityExpandoObject class.
		/// </summary>
		public UtilityExpandoObject() {
			// Initialize a new instance of the Dictionary class.
			_Dictionary = new Dictionary<string, object>(StringComparer.InvariantCultureIgnoreCase);
		}

		#region DynamicObject
		/// <summary>
		/// Provides the implementation for operations that delete an object member.
		/// </summary>
		/// <param name="Binder">Provides information about the deletion.</param>
		public override bool TryDeleteMember(DeleteMemberBinder Binder) {
			// Check if the dictionary contains the key.
			if (_Dictionary.ContainsKey(Binder.Name)) {
				// Remove the key from the dictionary.
				_Dictionary.Remove(Binder.Name);
				// Return true.
				return true;
			}
			// Invoke the base method.
			return base.TryDeleteMember(Binder);
		}

		/// <summary>
		/// Provides the implementation for operations that get member values.
		/// </summary>
		/// <param name="Binder">Provides information about the object that called the dynamic operation.</param>
		/// <param name="Result">The result of the get operation.</param>
		public override bool TryGetMember(GetMemberBinder Binder, out object Result) {
			// Check if the dictionary contains the key.
			if (_Dictionary.ContainsKey(Binder.Name)) {
				// Set the result of the operation.
				Result = this._Dictionary[Binder.Name];
				// Return true.
				return true;
			}
			// Invoke the base method.
			return base.TryGetMember(Binder, out Result);
		}

		/// <summary>
		/// Provides the implementation for operations that invoke a member.
		/// </summary>
		/// <param name="Binder">Provides information about the dynamic operation.</param>
		/// <param name="Arguments">The arguments that are passed to the object member during the invoke operation.</param>
		/// <param name="Result">The result of the member invocation.</param>
		public override bool TryInvokeMember(InvokeMemberBinder Binder, object[] Arguments, out object Result) {
			// Check if the dictionary contains the key and can be invoked.
			if (_Dictionary.ContainsKey(Binder.Name) && _Dictionary[Binder.Name] is Delegate) {
				// Invoke the delegate.
				Result = (_Dictionary[Binder.Name] as Delegate).DynamicInvoke(Arguments);
				// Return true.
				return true;
			}
			// Invoke the base method.
			return base.TryInvokeMember(Binder, Arguments, out Result);
		}

		/// <summary>
		/// Provides the implementation for operations that set member values.
		/// </summary>
		/// <param name="Binder">Provides information about the object that called the dynamic operation.</param>
		/// <param name="Value">The value to set to the member.</param>
		public override bool TrySetMember(SetMemberBinder Binder, object Value) {
			// Set the value in the dictionary.
			_Dictionary[Binder.Name] = Value;
			// Return true.
			return true;
		}
		#endregion

		#region ICollection<KeyValuePair<string, object>>
		/// <summary>
		/// Gets the number of elements contained in the collection.
		/// </summary>
		public int Count {
			get {
				return this._Dictionary.Keys.Count;
			}
		}

		/// <summary>
		/// Gets a value indicating whether the collection is read-only.
		/// </summary>
		public bool IsReadOnly {
			get {
				return _Dictionary.IsReadOnly;
			}
		}

		/// <summary>
		/// Adds an item to the collection.
		/// </summary>
		/// <param name="Item">The object to add to the collection.</param>
		public void Add(KeyValuePair<string, object> Item) {
			_Dictionary.Add(Item);
		}

		/// <summary>
		/// Removes all items from the collection.
		/// </summary>
		public void Clear() {
			_Dictionary.Clear();
		}

		/// <summary>
		/// Determines whether the collection contains a specific value.
		/// </summary>
		/// <param name="Item">The object to locate in the collection.</param>
		public bool Contains(KeyValuePair<string, object> Item) {
			return _Dictionary.Contains(Item);
		}

		/// <summary>
		/// Copies the elements of the collection to an array, starting at a particular array index.
		/// </summary>
		/// <param name="Array">The one-dimensional array that is the destination of the elements copied from the collection.</param>
		/// <param name="ArrayIndex">The zero-based index in array at which copying begins.</param>
		public void CopyTo(KeyValuePair<string, object>[] Array, int ArrayIndex) {
			_Dictionary.CopyTo(Array, ArrayIndex);
		}

		/// <summary>
		/// Removes the first occurrence of a specific object from the collection.
		/// </summary>
		/// <param name="Item">The object to remove from the collection.</param>
		public bool Remove(KeyValuePair<string, object> Item) {
			return _Dictionary.Remove(Item);
		}
		#endregion

		#region IDictionary<string, object>
		/// <summary>
		/// Gets a collection containing the keys of the dictionary.
		/// </summary>
		public ICollection<string> Keys {
			get {
				return _Dictionary.Keys;
			}
		}

		/// <summary>
		/// Gets a collection containing the values of the dictionary.
		/// </summary>
		public ICollection<object> Values {
			get {
				return _Dictionary.Values;
			}
		}

		/// <summary>
		/// Gets or sets the element with the specified key.
		/// </summary>
		/// <param name="Key">The key of the element to get or set.</param>
		public object this[string Key] {
			get {
				return _Dictionary[Key];
			}
			set {
				_Dictionary[Key] = value;
			}
		}

		/// <summary>
		/// Adds an element with the provided key and value to the dictionary.
		/// </summary>
		/// <param name="Key">The object to use as the key of the element to add.</param>
		/// <param name="Value">The object to use as the value of the element to add.</param>
		public void Add(string Key, object Value) {
			_Dictionary.Add(Key, Value);
		}

		/// <summary>
		/// Determines whether the dictionary contains an element with the specified key.
		/// </summary>
		/// <param name="Key">The key to locate in the dictionary.</param>
		public bool ContainsKey(string Key) {
			return _Dictionary.ContainsKey(Key);
		}

		/// <summary>
		/// Removes the element with the specified key from the dictionary.
		/// </summary>
		/// <param name="Key">The key of the element to remove.</param>
		public bool Remove(string Key) {
			return _Dictionary.Remove(Key);
		}

		/// <summary>
		/// Gets the value associated with the specified key.
		/// </summary>
		/// <param name="Key">The key whose value to get.</param>
		/// <param name="Value">The value associated with the specified key, if the key is found</param>
		public bool TryGetValue(string Key, out object Value) {
			return _Dictionary.TryGetValue(Key, out Value);
		}
		#endregion

		#region IEnumerable<KeyValuePair<string, object>>
		/// <summary>
		/// Returns an enumerator that iterates through the collection.
		/// </summary>
		public IEnumerator<KeyValuePair<string, object>> GetEnumerator() {
			return _Dictionary.GetEnumerator();
		}

		/// <summary>
		/// Returns an enumerator that iterates through a collection.
		/// </summary>
		/// <returns></returns>
		IEnumerator IEnumerable.GetEnumerator() {
			return GetEnumerator();
		}
		#endregion
	}
}