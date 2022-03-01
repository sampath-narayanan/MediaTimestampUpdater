using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using J4JSoftware.ExifTSUpdater;
using Microsoft.UI.Dispatching;

namespace MediaTimestampUpdater
{
    public class SharedFileInfo : ObservableCollection<MediaFileViewModel>, 
        IMultiThreadCollection<MediaFileViewModel>
    {
        private readonly DispatcherQueue _dQueue = DispatcherQueue.GetForCurrentThread();

        public string RootPath { get; set; } = string.Empty;

        public void DoAction( Action<ICollection<MediaFileViewModel>> action )
        {
            _dQueue.TryEnqueue( () => action( this ) );
        }

        public void CollectionReset() =>
            OnCollectionChanged( new NotifyCollectionChangedEventArgs( NotifyCollectionChangedAction.Reset ) );

        protected override void SetItem(int index, MediaFileViewModel item)
        {
            base.SetItem(index, item);

            item.RootPath = RootPath;
        }

        protected override void InsertItem(int index, MediaFileViewModel item)
        {
            base.InsertItem(index, item);

            item.RootPath = RootPath;
        }
    }
}
