﻿using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;

namespace FluentBootstrap
{
    public class Tag<TModel> : Component<TModel>
    {
        private readonly List<IComponent> _children = new List<IComponent>();

        internal TagBuilder TagBuilder { get; private set; }
        internal HashSet<string> CssClasses { get; private set; }

        internal string TextContent { get; set; }   // Can be used to set simple text content for the tag

        protected internal Tag(BootstrapHelper<TModel> helper, string tagName, params string[] cssClasses)
            : base(helper)
        {
            TagBuilder = new TagBuilder(tagName);
            CssClasses = new HashSet<string>();
            foreach (string cssClass in cssClasses)
                CssClasses.Add(cssClass);
        }

        internal Tag<TModel> AddChild(IComponent component)
        {
            _children.Add(component);
            PendingComponents.Remove(HtmlHelper, component); // Remove the pending child component because it's now associated with this one
            return this;
        }

        internal Tag<TModel> MergeAttributes(object attributes, bool replaceExisting = true)
        {
            if (attributes == null)
                return this;
            MergeAttributes(System.Web.Mvc.HtmlHelper.AnonymousObjectToHtmlAttributes(attributes), replaceExisting);
            return this;
        }

        internal Tag<TModel> MergeAttributes<TKey, TValue>(IDictionary<TKey, TValue> attributes, bool replaceExisting = true)
        {
            if (attributes == null)
                return this;
            foreach (KeyValuePair<TKey, TValue> attribute in attributes)
            {
                string key = Convert.ToString(attribute.Key, CultureInfo.InvariantCulture);
                string value = Convert.ToString(attribute.Value, CultureInfo.InvariantCulture);
                MergeAttribute(key, value, replaceExisting);
            }
            return this;
        }

        // This works a little bit differently then the TagBuilder.MergeAttribute() method
        // This version does not throw on null or whitespace key and removes the attribute if value is null
        internal Tag<TModel> MergeAttribute(string key, string value, bool replaceExisting = true)
        {
            if (string.IsNullOrWhiteSpace(key))
                return this;
            if (value == null && replaceExisting && TagBuilder.Attributes.ContainsKey(key))
            {
                TagBuilder.Attributes.Remove(key);
            }
            else if (value != null && (replaceExisting || !TagBuilder.Attributes.ContainsKey(key)))
            {
                TagBuilder.Attributes[key] = value;
            }
            return this;
        }

        internal Tag<TModel> ToggleCssClass(string cssClass, bool add, params string[] removeIfAdding)
        {
            if (add)
            {
                foreach (string remove in removeIfAdding)
                {
                    CssClasses.Remove(remove);
                }
                CssClasses.Add(cssClass);
            }
            else
            {
                CssClasses.Remove(cssClass);
            }
            return this;
        }

        protected override void Prepare(TextWriter writer)
        {
            base.Prepare(writer);

            // Add the text content as a child
            if (!string.IsNullOrEmpty(TextContent))
            {
                this.AddChild(new Content<TModel>(Helper, TextContent));
            }
        }

        protected override void OnStart(TextWriter writer)
        {
            // Set CSS classes
            foreach (string cssClass in CssClasses)
            {
                TagBuilder.AddCssClass(cssClass);
            }

            // Append the start tag
            writer.Write(TagBuilder.ToString(TagRenderMode.StartTag));

            // Append any children
            foreach (IComponent child in _children)
            {
                child.Start(writer, false);
                child.Finish(writer);
            }
        }

        protected override void OnFinish(TextWriter writer)
        {
            // Append the end tag
            writer.Write(TagBuilder.ToString(TagRenderMode.EndTag));
        }
    }
}
